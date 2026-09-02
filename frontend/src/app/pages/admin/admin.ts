import { Component, ChangeDetectorRef, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';

interface VenueBlock {
  id: number;
  name: string;
  type: number;
  capacity: number;
}

interface BlockPrice {
  venueBlockId: number;
  price: number | null;
}

interface EventItem {
  id: number;
  name: string;
  eventDate: string;
  venueId: number;
  blockPrices?: {
    id: number;
    venueBlockId: number;
    price: number;
  }[];
}

@Component({
  selector: 'app-admin',
  imports: [FormsModule, RouterLink, DatePipe],
  templateUrl: './admin.html',
  styleUrl: './admin.css'
})
export class Admin {
  private http = inject(HttpClient);
  private cdr = inject(ChangeDetectorRef);

  events: EventItem[] = [];

  venueBlocks: VenueBlock[] = [];
  blockPrices: BlockPrice[] = [];

  name = '';
  date = '';
  venueId: number | null = 1005;

  editingId: number | null = null;

  isLoading = true;
  isSaving = false;
  errorMessage = '';
  successMessage = '';

  ngOnInit() {
    this.loadVenueBlocks();
    this.loadEvents();
  }

  getHeaders() {
    const token = localStorage.getItem('token');

    return new HttpHeaders({
      Authorization: `Bearer ${token}`
    });
  }

  loadVenueBlocks() {
    if (!this.venueId) {
      this.venueBlocks = [];
      this.blockPrices = [];
      return;
    }

    this.http
      .get<any>(`http://localhost:5040/api/venue/${this.venueId}`)
      .subscribe({
        next: response => {
          this.venueBlocks = response.blocks ?? [];

          this.blockPrices = this.venueBlocks.map(block => ({
            venueBlockId: block.id,
            price: null
          }));

          this.cdr.detectChanges();
        },
        error: error => {
          console.error('Salon bilgileri yüklenemedi:', error);

          this.venueBlocks = [];
          this.blockPrices = [];

          this.errorMessage = 'Salon bilgileri yüklenemedi.';
          this.cdr.detectChanges();
        }
      });
  }

  loadEvents() {
    this.http
      .get<EventItem[]>('http://localhost:5040/api/events')
      .subscribe({
        next: response => {
          this.events = response;
          this.isLoading = false;
          this.cdr.detectChanges();
        },
        error: error => {
          console.error('Etkinlikler yüklenemedi:', error);

          this.errorMessage = 'Etkinlikler yüklenemedi.';
          this.isLoading = false;

          this.cdr.detectChanges();
        }
      });
  }

  saveEvent() {
    this.errorMessage = '';
    this.successMessage = '';

    if (!this.name || !this.date || !this.venueId) {
      this.errorMessage =
        'Lütfen etkinlik adı, tarih ve salon bilgilerini doldurun.';
      return;
    }

    if (this.blockPrices.length === 0) {
      this.errorMessage =
        'Seçilen salonda fiyatlandırılabilir blok bulunamadı.';
      return;
    }

    if (
      this.blockPrices.some(
        block => block.price === null || block.price < 0
      )
    ) {
      this.errorMessage = 'Lütfen tüm bilet fiyatlarını girin.';
      return;
    }

    this.isSaving = true;

    const blocks: BlockPrice[] = this.blockPrices.map(block => ({
      venueBlockId: block.venueBlockId,
      price: Number(block.price)
    }));

    const eventData = {
      name: this.name,
      eventDate: this.date,
      venueId: Number(this.venueId),
      blocks: blocks
    };

    if (this.editingId === null) {
      this.http
        .post(
          'http://localhost:5040/api/events',
          eventData,
          {
            headers: this.getHeaders()
          }
        )
        .subscribe({
          next: () => {
            this.successMessage =
              'Etkinlik başarıyla oluşturuldu.';

            this.clearForm();
            this.loadEvents();
          },
          error: error => {
            console.error(
              'Etkinlik oluşturma hatası:',
              error
            );

            this.errorMessage =
              error.error?.error ||
              'Etkinlik oluşturulamadı.';

            this.isSaving = false;

            this.cdr.detectChanges();
          }
        });
    } else {
      this.http
        .put(
          `http://localhost:5040/api/events/${this.editingId}`,
          eventData,
          {
            headers: this.getHeaders()
          }
        )
        .subscribe({
          next: () => {
            this.successMessage =
              'Etkinlik başarıyla güncellendi.';

            this.clearForm();
            this.loadEvents();
          },
          error: error => {
            console.error(
              'Etkinlik güncelleme hatası:',
              error
            );

            this.errorMessage =
              error.error?.error ||
              'Etkinlik güncellenemedi.';

            this.isSaving = false;

            this.cdr.detectChanges();
          }
        });
    }
  }

  editEvent(event: EventItem) {
    this.editingId = event.id;

    this.name = event.name;
    this.date = event.eventDate;
    this.venueId = event.venueId;

    this.errorMessage = '';
    this.successMessage = '';

    this.http
      .get<any>(
        `http://localhost:5040/api/venue/${event.venueId}`
      )
      .subscribe({
        next: response => {
          this.venueBlocks = response.blocks ?? [];

          this.blockPrices = this.venueBlocks.map(block => ({
            venueBlockId: block.id,
            price:
              event.blockPrices?.find(
                price => price.venueBlockId === block.id
              )?.price ?? null
          }));

          this.cdr.detectChanges();
        },
        error: error => {
          console.error(
            'Salon bilgileri yüklenemedi:',
            error
          );

          this.errorMessage =
            'Etkinlik blok bilgileri yüklenemedi.';

          this.cdr.detectChanges();
        }
      });
  }

  deleteEvent(id: number) {
    if (
      !confirm(
        'Bu etkinliği silmek istediğinize emin misiniz?'
      )
    ) {
      return;
    }

    this.errorMessage = '';
    this.successMessage = '';

    this.http
      .delete(
        `http://localhost:5040/api/events/${id}`,
        {
          headers: this.getHeaders()
        }
      )
      .subscribe({
        next: () => {
          this.successMessage =
            'Etkinlik başarıyla silindi.';

          this.loadEvents();
        },
        error: error => {
          console.error(
            'Etkinlik silme hatası:',
            error
          );

          this.errorMessage =
            error.error?.error ||
            'Etkinlik silinemedi.';

          this.cdr.detectChanges();
        }
      });
  }

  clearForm() {
    this.name = '';
    this.date = '';

    this.venueId = 1005;

    this.venueBlocks = [];

    this.blockPrices = [];

    this.editingId = null;

    this.isSaving = false;

    this.cdr.detectChanges();

    // Yeni etkinlik formu açıldığında
    // varsayılan salonun bloklarını tekrar getir.
    this.loadVenueBlocks();
  }
}