import { useUserStore } from 'src/stores/user-store'
import type { NavigationGuard } from 'vue-router'

export const guardIsConnected: NavigationGuard = (to, from, next) => {
  const userStore = useUserStore()
  if (!userStore.isConnected) {
    next({ name: 'login' })
    return
  }
  next()
}
export const guardIsDisconnected: NavigationGuard = (to, from, next) => {
  const userStore = useUserStore()
  if (userStore.isConnected) {
    next({ name: 'home' })
    return
  }
  next()
}
export const guardEnterApp: NavigationGuard = async (to, from, next) => {
  const userStore = useUserStore()
  if (userStore.hasEnteredApp) {
    next()
    return
  }
  userStore.hasEnteredApp = true
  userStore.getInitialPathRequested(to.fullPath)
  if (!userStore.isConnected) {
    userStore.trySetupTokenFromCookies()
  }
  const urlWithTicket = window.location.search.match(/ST.+/)
  if (!userStore.isConnected && urlWithTicket !== null && (urlWithTicket?.length ?? 0 > 0)) {
    const ticketFromCas = urlWithTicket[0]
    await userStore.askServerToken(ticketFromCas)
  }
  if (!userStore.isConnected) {
    next({ name: 'login' })
    return
  } else {
    const cookieRoute = userStore.getCookieRouteValue
    if (cookieRoute !== null && cookieRoute !== '/' && cookieRoute !== '/login') {
      next(cookieRoute)
      return
    }
    next({ name: 'home' })
  }
}
