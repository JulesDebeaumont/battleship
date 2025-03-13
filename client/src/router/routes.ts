import type { RouteRecordRaw } from 'vue-router'
import { guardEnterApp, guardEnterFakeApp, guardIsConnected, guardIsDisconnected } from './guards'

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    name: 'fake-root',
    beforeEnter: guardEnterFakeApp,
    children: [
      {
        path: '/login',
        name: 'fake-login',
        component: () => import('pages/FakeLoginPage.vue'),
      },
    ],
  },
  {
    path: `/${process.env.SECRET_URL_SEGMENT}`,
    name: 'root',
    beforeEnter: guardEnterApp,
    component: () => import('layouts/MainLayout.vue'),
    children: [
      {
        path: 'login',
        name: 'login',
        beforeEnter: guardIsDisconnected,
        component: () => import('pages/LoginPage.vue'),
        meta: { layoutLogo: 'important' },
      },
      {
        path: 'home',
        beforeEnter: guardIsConnected,
        children: [
          {
            path: '',
            name: 'home',
            component: () => import('pages/HomePage.vue'),
            meta: { layoutLogo: 'important' },
          },
          {
            path: 'search-room',
            name: 'search-room',
            component: () => import('pages/SearchRoomPage.vue'),
          },
          {
            path: 'leaderboard',
            name: 'leaderboard',
            component: () => import('pages/LeaderboardPage.vue'),
          },
          {
            path: 'room/:guid/opponent',
            name: 'room-as-opponent',
            component: () => import('pages/RoomFightPage.vue'),
          },
          {
            path: 'room/:guid/spectator',
            name: 'room-as-spectator',
            component: () => import('pages/RoomSpectatorPage.vue'),
          },
          {
            path: 'me',
            name: 'me',
            component: () => import('pages/UserProfilPage.vue'),
          },
          {
            path: 'changelog',
            name: 'changelog',
            component: () => import('pages/ChangeLogPage.vue'),
          },
        ],
      },
    ],
  },

  // Always leave this as last one,
  // but you can also remove it
  {
    path: '/:catchAll(.*)*',
    component: () => import('pages/ErrorNotFound.vue'),
  },
]

export default routes
