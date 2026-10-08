import { Component, ChangeDetectorRef, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { DatePipe, CurrencyPipe } from '@angular/common';
import { RouterLink } from '@angular/router';

interface OrderItem {
  id: number;
  eventSeatId: number;
  price: number;
  ticketId: number;
  ticketNumber: string;
  status: 'Active' | 'Cancelled';
  cancelledAt?: string | null;
}

interface Order {
  id: number;
  userId: number;
  totalAmount: number;
  createdAt: string;
  items: OrderItem[];
}

@Component({
  selector: 'app-orders',
  imports: [DatePipe, CurrencyPipe, RouterLink],
  templateUrl: './orders.html',
  styleUrl: './orders.css'
})
export class Orders {
  private http = inject(HttpClient);
  private cdr = inject(ChangeDetectorRef);

  orders: Order[] = [];
  isLoading = true;
  errorMessage = '';

  ngOnInit() {
    this.loadOrders();
  }

  loadOrders() {
    const token = localStorage.getItem('token');

    if (!token) {
      this.errorMessage = 'Siparişlerinizi görmek için giriş yapmalısınız.';
      this.isLoading = false;
      return;
    }

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    this.http
      .get<Order[]>('http://localhost:5040/api/orders', { headers })
      .subscribe({
        next: (response) => {
          this.orders = response;
          this.isLoading = false;
          this.cdr.detectChanges();
        },
        error: (error) => {
          console.error('Sipariş API hatası:', error);

          if (error.status === 401) {
            this.errorMessage = 'Oturumunuz geçersiz. Lütfen tekrar giriş yapın.';
          } else {
            this.errorMessage = 'Siparişler yüklenemedi.';
          }

          this.isLoading = false;
          this.cdr.detectChanges();
        }
      });
  }

  cancelTicket(ticketId: number) {
    if (!ticketId) return;

    const confirmed = confirm('Bu bileti iptal etmek istediğinize emin misiniz? Koltuk tekrar satışa açılacaktır.');
    if (!confirmed) return;

    const token = localStorage.getItem('token');
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    this.http
      .post<{ message: string }>(`http://localhost:5040/api/orders/tickets/${ticketId}/cancel`, {}, { headers })
      .subscribe({
        next: (res) => {
          alert(res.message || 'Bilet başarıyla iptal edildi.');
          this.loadOrders(); // Listeyi anlık güncelle
        },
        error: (err) => {
          const msg = err.error?.message || 'Bilet iptal edilirken bir hata oluştu.';
          alert(msg);
        }
      });
  }
}