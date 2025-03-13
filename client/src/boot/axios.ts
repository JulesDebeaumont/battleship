import { defineBoot } from '#q-app/wrappers'
import axios, { type AxiosInstance } from 'axios'
import { Notify } from 'quasar'
import { useUserStore } from 'src/stores/user-store'
import { useRouter } from 'vue-router'

declare module 'vue' {
  interface ComponentCustomProperties {
    $axios: AxiosInstance
    $api: AxiosInstance
  }
}

// Be careful when using SSR for cross-request state pollution
// due to creating a Singleton instance here;
// If any client changes this (global) instance, it might be a
// good idea to move this instance creation inside of the
// "export default () => {}" function below (which runs individually
// for each client)
const api = axios.create({ baseURL: process.env.ENDPOINT_API ?? '' })

export default defineBoot(() => {
  const userStore = useUserStore()

  api.interceptors.request.use((config) => {
    if (userStore.isConnected) {
      if (userStore.istokenExpired) {
        userStore.clear()
        return config
      }
      config.headers.Authorization = `Bearer ${userStore.token}`
    }
    return config
  })

  api.interceptors.response.use(
    (response) => {
      return response
    },
    async (error) => {
      if (error.status === 401 || error.status === 403) {
        const router = useRouter()
        await router.push({ name: 'home' })
        return error
      }
      Notify.create({
        color: 'negative',
        message: 'Une erreur est survenue',
      })
      return error
    },
  )
})

export { api }
