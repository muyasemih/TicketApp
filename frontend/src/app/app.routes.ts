import { Routes } from '@angular/router';
import { authGuard } from './guards/auth-guard';
import { adminGuard } from './guards/admin-guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'events',
    pathMatch: 'full'
  },
  {
    path: 'login',
    loadComponent: () =>
      import('./pages/login/login').then(m => m.Login)
  },
  {
  path: 'register',
  loadComponent: () =>
    import('./pages/register/register').then(m => m.Register)
},
  {
    path: 'events',
    loadComponent: () =>
      import('./pages/events/events').then(m => m.Events)
  },
  {
    path: 'events/:id',
    loadComponent: () =>
      import('./pages/event-detail/event-detail').then(m => m.EventDetail)
  },
  {
    path: 'events/:id/seats',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./pages/seats/seats').then(m => m.Seats)
  },
  {
  path: 'admin',
  canActivate: [adminGuard],
  loadComponent: () =>
    import('./pages/admin/admin').then(m => m.Admin)
  },
  {
    path: 'orders',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./pages/orders/orders').then(m => m.Orders)
  },
  {
    path: '**',
    redirectTo: 'events'
  }
];