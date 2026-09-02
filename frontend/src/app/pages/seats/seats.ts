import { Component, ChangeDetectorRef, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ActivatedRoute, RouterLink } from '@angular/router';

interface Seat {
  id: number;
  eventId: number;
  seatId: number;
  status: string;
  reservedUntil: string | null;

  // Asıl Seat nesnesi backend'den geliyor.
  seat?: {
    id: number;
    rowNumber: number;
    seatNumber: number;
    number: number;
    venueBlockId: number;
  };

  blockId: number;
  blockName: string;
  blockType: number;
}

interface VenueBlock {
  id: number;
  name: string;
  type: number;
  capacity: number;
}

interface Venue {
  id: number;
  name: string;
  blocks: VenueBlock[];
}

@Component({
  selector: 'app-seats',
  imports: [RouterLink],
  templateUrl: './seats.html',
  styleUrl: './seats.css'
})
export class Seats {
  private http = inject(HttpClient);
  private route = inject(ActivatedRoute);
  private cdr = inject(ChangeDetectorRef);

  seats: Seat[] = [];

  isLoading = true;
  isReserving = false;
  errorMessage = '';

  selectedSeat: Seat | null = null;

  standingSeat: Seat | null = null;
  standingAvailableCount = 0;
  hasStanding = false;

  ngOnInit() {
    const eventId = this.route.snapshot.paramMap.get('id');

    if (!eventId) {
      this.errorMessage = 'Etkinlik bulunamadı.';
      this.isLoading = false;
      return;
    }

    this.loadSeats(eventId);
  }

  loadSeats(eventId: string) {
    this.http
      .get<Seat[]>(
        `http://localhost:5040/api/events/${eventId}/seats`
      )
      .subscribe({
        next: (response) => {
          console.log('Koltuklar:', response);
          this.loadVenueInfo(eventId, response);
        },
        error: (error) => {
          console.error('Koltuk API hatası:', error);

          this.errorMessage = 'Koltuklar yüklenemedi.';
          this.isLoading = false;

          this.cdr.detectChanges();
        }
      });
  }

  loadVenueInfo(eventId: string, eventSeats: Seat[]) {
    this.http
      .get<any>(
        `http://localhost:5040/api/events/${eventId}`
      )
      .subscribe({
        next: (event) => {
          const venue = event.venue as Venue | null;

          if (!venue) {
            this.seats = eventSeats;

            this.prepareStandingArea();

            this.isLoading = false;
            this.cdr.detectChanges();

            return;
          }

          // Blokları ID üzerinden hızlıca bulabileceğimiz Map oluşturuyoruz.
          const blockMap = new Map<number, VenueBlock>();

          for (const block of venue.blocks ?? []) {
            blockMap.set(block.id, block);
          }

          /*
           * ÖNEMLİ:
           *
           * Daha önce venue.blocks[].seats üzerinden eşleştirme yapıyorduk.
           * Bu yüzden koltukların blok bilgisi bulunamıyordu ve hepsi
           * A Blok gibi görünüyordu.
           *
           * Şimdi doğrudan EventSeat içerisindeki:
           * seat.venueBlockId
           *
           * değerini kullanıyoruz.
           */
          this.seats = eventSeats.map(seat => {
            const venueBlockId =
              (seat as any).venueBlockId;

            const block = blockMap.get(venueBlockId);

            return {
              ...seat,

              blockId: block?.id ?? 0,
              blockName: block?.name ?? 'Bilinmeyen Blok',
              blockType: block?.type ?? 0
            };
          });

          console.log(
            'Blok bilgileri eklenmiş koltuklar:',
            this.seats
          );

          this.prepareStandingArea();

          this.isLoading = false;

          this.cdr.detectChanges();
        },

        error: (error) => {
          console.error(
            'Etkinlik bilgileri yüklenemedi:',
            error
          );

          this.seats = eventSeats;

          this.prepareStandingArea();

          this.isLoading = false;

          this.cdr.detectChanges();
        }
      });
  }

  prepareStandingArea() {
    const standingSeats = this.seats.filter(
      seat =>
        seat.blockType === 1 ||
        seat.blockName === 'Ayakta Alan'
    );

    this.hasStanding = standingSeats.length > 0;

    this.standingAvailableCount =
      standingSeats.filter(
        seat => seat.status === 'Available'
      ).length;

    this.standingSeat =
      standingSeats.find(
        seat => seat.status === 'Available'
      ) ?? null;
  }

  reserveSeat(seat: Seat) {
    if (
      seat.status !== 'Available' ||
      this.isReserving
    ) {
      return;
    }

    const eventId =
      this.route.snapshot.paramMap.get('id');

    if (!eventId) {
      this.errorMessage = 'Etkinlik bulunamadı.';
      return;
    }

    const token = localStorage.getItem('token');

    if (!token) {
      this.errorMessage =
        'Rezervasyon yapmak için giriş yapmalısınız.';
      return;
    }

    this.isReserving = true;
    this.errorMessage = '';

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    this.http
      .post(
        `http://localhost:5040/api/events/${eventId}/seats/${seat.seatId}/reserve`,
        {},
        { headers }
      )
      .subscribe({
        next: (response) => {
          console.log(
            'Rezervasyon başarılı:',
            response
          );

          seat.status = 'Reserved';

          this.selectedSeat = seat;

          this.isReserving = false;

          this.prepareStandingArea();

          this.cdr.detectChanges();
        },

        error: (error) => {
          console.error(
            'Rezervasyon hatası:',
            error
          );

          this.isReserving = false;

          if (error.status === 401) {
            this.errorMessage =
              'Oturumunuz geçersiz. Lütfen tekrar giriş yapın.';
          } else if (error.status === 400) {
            this.errorMessage =
              'Bu koltuk artık müsait değil veya rezerve edilemedi.';
          } else {
            this.errorMessage =
              'Rezervasyon sırasında bir hata oluştu.';
          }

          this.cdr.detectChanges();
        }
      });
  }

  purchaseSeat() {
    if (!this.selectedSeat) {
      return;
    }

    const eventId =
      this.route.snapshot.paramMap.get('id');

    if (!eventId) {
      this.errorMessage = 'Etkinlik bulunamadı.';
      return;
    }

    const token = localStorage.getItem('token');

    if (!token) {
      this.errorMessage =
        'Satın alma işlemi için giriş yapmalısınız.';
      return;
    }

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    const orderData = {
      eventId: Number(eventId),
      eventSeatIds: [this.selectedSeat.id]
    };

    this.http
      .post(
        'http://localhost:5040/api/orders',
        orderData,
        { headers }
      )
      .subscribe({
        next: (response) => {
          console.log(
            'Sipariş başarılı:',
            response
          );

          this.selectedSeat!.status = 'Sold';

          this.selectedSeat = null;

          this.prepareStandingArea();

          this.cdr.detectChanges();

          alert(
            'Biletiniz başarıyla oluşturuldu!'
          );
        },

        error: (error) => {
          console.error(
            'Sipariş hatası:',
            error
          );

          if (error.status === 409) {
            this.errorMessage =
              'Bu koltuk artık kullanılamıyor veya rezervasyon süresi dolmuş.';
          } else if (error.status === 401) {
            this.errorMessage =
              'Oturumunuz geçersiz. Lütfen tekrar giriş yapın.';
          } else {
            this.errorMessage =
              'Satın alma sırasında bir hata oluştu.';
          }

          this.cdr.detectChanges();
        }
      });
  }
}