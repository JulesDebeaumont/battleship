import type { RouteRecordRaw } from 'vue-router';
import { guardIsConnected, guardIsDisconnected } from './guards';

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    name: 'root',
    redirect: { name: 'home'}
  },
  {
    path: '/login',
    name: 'login',
    beforeEnter: guardIsDisconnected,
    component: () => import('pages/LoginPage.vue')
  },
  {
    path: '/home',
    beforeEnter: guardIsConnected,
    children: [
      {
        path: '',
        name: 'home',
        component: () => import('pages/HomePage.vue')
      },
      {
        path: 'room/:guid/:role',
        name: 'room',
        component: () => import('pages/RoomPage.vue')
      }
    ]
  },

  // Always leave this as last one,
  // but you can also remove it
  {
    path: '/:catchAll(.*)*',
    component: () => import('pages/ErrorNotFound.vue'),
  },
];

export default routes;
