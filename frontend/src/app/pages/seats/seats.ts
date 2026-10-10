import { Component, ChangeDetectorRef, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ActivatedRoute, RouterLink, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';

interface Seat {
  id: number;
  eventId: number;
  seatId: number;
  status: string;
  reservedUntil: string | null;

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
  imports: [RouterLink, FormsModule],
  templateUrl: './seats.html',
  styleUrl: './seats.css'
})
export class Seats {
  private http = inject(HttpClient);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);

  seats: Seat[] = [];
  numberedSeats: Seat[] = [];

  isLoading = true;
  isReserving = false;
  isProcessingPayment = false;
  errorMessage = '';

  selectedSeat: Seat | null = null;

  standingSeat: Seat | null = null;
  standingAvailableCount = 0;
  standingTotalCount = 0;
  hasStanding = false;

  // Ödeme Modalı & Form Alanları
  showPaymentModal = false;
  paymentError = '';
  paymentForm = {
    cardHolderName: '',
    cardNumber: '',
    expireMonth: '12',
    expireYear: '28',
    cvv: '123'
  };

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
            this.updateSeatLists();
            this.isLoading = false;
            this.cdr.detectChanges();
            return;
          }

          const blockMap = new Map<number, VenueBlock>();
          for (const block of venue.blocks ?? []) {
            blockMap.set(block.id, block);
          }

          this.seats = eventSeats.map(seat => {
            const venueBlockId =
              seat.blockId ??
              seat.seat?.venueBlockId ??
              (seat as any).venueBlockId;

            const block = blockMap.get(venueBlockId);

            return {
              ...seat,
              blockId: seat.blockId ?? block?.id ?? 0,
              blockName: seat.blockName || block?.name || 'Bilinmeyen Blok',
              blockType: seat.blockType ?? block?.type ?? 0
            };
          });

          this.updateSeatLists();
          this.isLoading = false;
          this.cdr.detectChanges();
        },
        error: (error) => {
          console.error('Etkinlik bilgileri yüklenemedi:', error);
          this.seats = eventSeats;
          this.updateSeatLists();
          this.isLoading = false;
          this.cdr.detectChanges();
        }
      });
  }

  updateSeatLists() {
    this.numberedSeats = this.seats.filter(
      seat =>
        seat.blockType !== 1 &&
        seat.blockName !== 'Ayakta Alan' &&
        (seat.seat?.rowNumber ?? 1) > 0
    );

    this.prepareStandingArea();
  }

  prepareStandingArea() {
    const standingSeats = this.seats.filter(
      seat =>
        seat.blockType === 1 ||
        seat.blockName === 'Ayakta Alan' ||
        (seat.seat?.rowNumber ?? 1) === 0
    );

    this.hasStanding = standingSeats.length > 0;
    this.standingTotalCount = standingSeats.length;
    this.standingAvailableCount = standingSeats.filter(
      seat => seat.status === 'Available'
    ).length;

    this.standingSeat =
      standingSeats.find(
        seat => seat.status === 'Available'
      ) ?? null;
  }

  reserveSeat(seat: Seat) {
    if (seat.status !== 'Available' || this.isReserving) {
      return;
    }

    const eventId = this.route.snapshot.paramMap.get('id');
    if (!eventId) {
      this.errorMessage = 'Etkinlik bulunamadı.';
      return;
    }

    const token = localStorage.getItem('token');
    if (!token) {
      this.errorMessage = 'Rezervasyon yapmak için giriş yapmalısınız.';
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
          seat.status = 'Reserved';
          this.selectedSeat = seat;
          this.isReserving = false;
          this.updateSeatLists();
          this.cdr.detectChanges();
        },
        error: (error) => {
          console.error('Rezervasyon hatası:', error);
          this.isReserving = false;
          if (error.status === 401) {
            this.errorMessage = 'Oturumunuz geçersiz. Lütfen tekrar giriş yapın.';
          } else if (error.status === 400) {
            this.errorMessage = 'Bu koltuk artık müsait değil veya rezerve edilemedi.';
          } else {
            this.errorMessage = 'Rezervasyon sırasında bir hata oluştu.';
          }
          this.cdr.detectChanges();
        }
      });
  }

  openPaymentModal() {
    this.paymentError = '';
    this.showPaymentModal = true;
  }

  closePaymentModal() {
    if (!this.isProcessingPayment) {
      this.showPaymentModal = false;
    }
  }

  submitPayment() {
    if (!this.selectedSeat) return;

    const eventId = this.route.snapshot.paramMap.get('id');
    const token = localStorage.getItem('token');

    if (!token || !eventId) {
      this.paymentError = 'Oturumunuz bulunamadı. Lütfen giriş yapın.';
      return;
    }

    if (!this.paymentForm.cardHolderName || !this.paymentForm.cardNumber || !this.paymentForm.cvv) {
      this.paymentError = 'Lütfen tüm kart bilgilerini eksiksiz doldurun.';
      return;
    }

    this.isProcessingPayment = true;
    this.paymentError = '';

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    const orderPayload = {
      eventId: Number(eventId),
      eventSeatIds: [this.selectedSeat.id],
      payment: {
        cardHolderName: this.paymentForm.cardHolderName,
        cardNumber: this.paymentForm.cardNumber.replace(/\s+/g, ''),
        expireMonth: this.paymentForm.expireMonth.padStart(2, '0'),
        expireYear: this.paymentForm.expireYear,
        cvv: this.paymentForm.cvv
      }
    };
    

    this.http
      .post('http://localhost:5040/api/orders', orderPayload, { headers })
      .subscribe({
        next: (response) => {
          this.isProcessingPayment = false;
          this.showPaymentModal = false;
          this.selectedSeat!.status = 'Sold';
          this.selectedSeat = null;
          this.updateSeatLists();
          this.cdr.detectChanges();

          alert('Ödeme onaylandı! Biletiniz başarıyla oluşturuldu.');
          this.router.navigate(['/orders']);

        },
        error: (error) => {
                  this.isProcessingPayment = false;
                  console.error('Ödeme / Sipariş hatası:', error);

                  // 1. Modalı kapat
                  this.showPaymentModal = false;

                  // 2. Hata mesajını belirle ve ana sayfada göster
                  if (error.status === 400 || error.status === 422) {
                    this.errorMessage = 'Ödeme reddedildi: Kart bilgileri geçersiz veya bakiye yetersiz.';
                  } else if (error.status === 409) {
                    this.errorMessage = 'Rezervasyon süresi doldu veya koltuk başka bir kullanıcı tarafından alındı.';
                  } else {
                    this.errorMessage = 'Ödeme işlemi sırasında bir hata oluştu. Lütfen tekrar deneyin.';
                  }

                  // 3. Koltuğu ve kart numarasını sıfırla, listeyi tazele
                  const eventId = this.route.snapshot.paramMap.get('id');
                  if (eventId) {
                    this.loadSeats(eventId);
                  }
                  this.selectedSeat = null;
                  this.paymentForm.cardNumber = '';

                  this.cdr.detectChanges();
                }
      });
      }formatCardNumber(event: Event) {
        const input = event.target as HTMLInputElement;
        let value = input.value.replace(/\D/g, '');

        if (value.length > 16) {
          value = value.substring(0, 16);
        }

        const parts = value.match(/.{1,4}/g);
        this.paymentForm.cardNumber = parts ? parts.join(' ') : value;
      }
  }
