import { Component, ChangeDetectorRef, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute, RouterLink } from '@angular/router';

interface VenueBlock {
  id: number;
  name: string;
  type: number;
  capacity: number;
}

interface Event {
  id: number;
  name: string;
  date: string;
  eventDate?: string;
  venueId: number;

  blockPrices?: {
    venueBlockId: number;
    price: number;
  }[];

  venue?: {
    blocks: VenueBlock[];
  };
}

interface DisplayBlockPrice {
  blockName: string;
  price: number;
}

@Component({
  selector: 'app-event-detail',
  imports: [RouterLink],
  templateUrl: './event-detail.html',
  styleUrl: './event-detail.css'
})
export class EventDetail {
  private http = inject(HttpClient);
  private route = inject(ActivatedRoute);
  private cdr = inject(ChangeDetectorRef);

  event: Event | null = null;
  displayPrices: DisplayBlockPrice[] = [];

  isLoading = true;
  errorMessage = '';

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');

    if (!id) {
      this.errorMessage = 'Etkinlik bulunamadı.';
      this.isLoading = false;
      return;
    }

    this.http
      .get<Event>(`http://localhost:5040/api/events/${id}`)
      .subscribe({
        next: (response) => {
          console.log('Etkinlik detayı:', response);

          this.event = response;

          this.displayPrices = (response.blockPrices ?? []).map(blockPrice => {

            const block = response.venue?.blocks?.find(
              venueBlock => venueBlock.id === blockPrice.venueBlockId
            );

            return {
              blockName: block?.name ?? `Blok ${blockPrice.venueBlockId}`,
              price: blockPrice.price
            };
          });

          this.isLoading = false;
          this.cdr.detectChanges();
        },

        error: (error) => {
          console.error('Etkinlik detay hatası:', error);

          this.errorMessage = 'Etkinlik bilgileri yüklenemedi.';
          this.isLoading = false;
          this.cdr.detectChanges();
        }
      });
  }
}