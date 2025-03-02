import { defineBoot } from '#q-app/wrappers';
import axios, { type AxiosInstance } from 'axios';
import { useUserStore } from 'src/stores/user-store';

declare module 'vue' {
  interface ComponentCustomProperties {
    $axios: AxiosInstance;
    $api: AxiosInstance;
  }
}

// Be careful when using SSR for cross-request state pollution
// due to creating a Singleton instance here;
// If any client changes this (global) instance, it might be a
// good idea to move this instance creation inside of the
// "export default () => {}" function below (which runs individually
// for each client)
const api = axios.create({ baseURL: process.env.ENDPOINT_API ?? '' });

export default defineBoot(() => {
  const userStore = useUserStore();

  api.interceptors.request.use(
    (config) => {
      if (userStore.isConnected) {
        if (userStore.istokenExpired) {
          userStore.clear();
          return config;
        }
        config.headers.Authorization = `Bearer ${userStore.token}`;
      }
      return config;
    }
  )
})

export { api };
