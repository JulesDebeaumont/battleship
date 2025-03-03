import { jwtDecode } from 'jwt-decode'
import { defineStore } from 'pinia'
import { Cookies } from 'quasar'
import { fakeLoginAPI, loginAPI } from 'src/api/login.api'
import { useRoute } from 'vue-router'

interface IUser {
  id: number
  pseudo: string
}
interface IParsedToken {
  exp: number
  user: IUser
}
const COOKIE_TOKEN = 'SpaceShipBattle-AccessToken'
const COOKIE_ROUTE_ENTERING = 'SpaceShipBattle-RequestedRoute'
const COOKIE_PATH = '/'
const SAME_SITE = 'Lax'

export const useUserStore = defineStore('user', {
  state: () => ({
    user: <IUser | null>null,
    token: <string | null>null,
    tokenExpiration: <number | null>null,
  }),
  getters: {
    isConnected(): boolean {
      return this.user !== null
    },
    istokenExpired(): boolean {
      if (this.tokenExpiration === null) return true
      if (new Date() > new Date(this.tokenExpiration * 1000)) return true
      return false
    },
  },
  actions: {
    async trySetupTokenFromCookies() {
      if (Cookies.has(COOKIE_TOKEN)) {
        await this.authenticate(Cookies.get(COOKIE_TOKEN))
      }
    },
    clear() {
      this.user = null
      this.token = null
      Cookies.remove(COOKIE_TOKEN, { path: COOKIE_PATH })
    },
    askCasTicket() {
      window.location.href = process.env.CAS_URL + window.location.origin
    },
    getInitialPathRequested() {
      const route = useRoute()
      Cookies.set(COOKIE_ROUTE_ENTERING, route.fullPath, { sameSite: SAME_SITE, path: COOKIE_PATH })
    },
    async askServerToken(casTicket: string) {
      try {
        const token = (await loginAPI(casTicket)).token;
        await this.authenticate(token);
      } catch (error) {
        console.log(error)
      }
    },
    async fakeAuth(idRes: string) {
      const jwt = (await fakeLoginAPI(idRes)).token
      await this.authenticate(jwt);
    },
    async authenticate(token: string) {
      this.token = token
      const parsedToken = jwtDecode<IParsedToken>(token)
      this.user = parsedToken.user
      this.tokenExpiration = parsedToken.exp
      Cookies.set(COOKIE_TOKEN, token, { sameSite: SAME_SITE, path: COOKIE_PATH })
      await this.router.push(Cookies.get(COOKIE_ROUTE_ENTERING));
    },
  },
})
