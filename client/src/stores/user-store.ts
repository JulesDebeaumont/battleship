import { jwtDecode } from 'jwt-decode'
import { defineStore } from 'pinia'
import { Cookies } from 'quasar'
import { loginAPI } from 'src/api/login.api'

interface IUser {
  id: number
  pseudo: string
}
interface IParsedToken {
  exp: number
  id: number
  pseudo: string
}
const COOKIE_TOKEN = 'SpaceShipBattle-AccessToken'
const COOKIE_ROUTE_ENTERING = 'SpaceShipBattle-RequestedRoute'
const COOKIE_PATH = '/'
const SAME_SITE = 'Lax'

export const useUserStore = defineStore('user', {
  state: () => ({
    hasEnteredApp: false,
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
    getCookieRouteValue(): string | null {
      return Cookies.get(COOKIE_ROUTE_ENTERING)
    },
  },
  actions: {
    trySetupTokenFromCookies() {
      if (Cookies.has(COOKIE_TOKEN)) {
        this.authenticate(Cookies.get(COOKIE_TOKEN))
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
    getInitialPathRequested(fullPath: string) {
      Cookies.set(COOKIE_ROUTE_ENTERING, fullPath, { sameSite: SAME_SITE, path: COOKIE_PATH })
    },
    async askServerToken(casTicket: string) {
      try {
        const token = (await loginAPI(casTicket)).token
        this.authenticate(token)
      } catch (error) {
        console.error(error)
      }
    },
    // async fakeAuth(idRes: string) {
    //   const jwt = (await fakeLoginAPI(idRes)).token
    //   this.authenticate(jwt);
    // },
    authenticate(token: string) {
      this.token = token
      const parsedToken = jwtDecode<IParsedToken>(token)
      this.user = {
        id: parsedToken.id,
        pseudo: parsedToken.pseudo,
      }
      this.tokenExpiration = parsedToken.exp
      Cookies.set(COOKIE_TOKEN, token, { sameSite: SAME_SITE, path: COOKIE_PATH })
    },
    logout() {
      this.$reset()
      Cookies.remove(COOKIE_TOKEN, { path: COOKIE_PATH })
      Cookies.remove(COOKIE_ROUTE_ENTERING, { path: COOKIE_PATH })
    },
  },
})
