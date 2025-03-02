import { useUserStore } from "src/stores/user-store";

export function guardIsConnected() {
    const userStore = useUserStore();
    if (!userStore.isConnected) {
        return { name: 'login' }
    }
}
export function guardIsDisconnected() {
    const userStore = useUserStore();
    if (userStore.isConnected) {
        return { name: 'home' }
    }
}
