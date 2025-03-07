import { HubConnectionBuilder } from '@microsoft/signalr'
import { useUserStore } from 'src/stores/user-store'
import { ref } from 'vue'

export function useSignalR(
  hubName: string,
) {
  const url = `${process.env.ENDPOINT_API}/ws/${hubName}`
  const connection = new HubConnectionBuilder()
    .withUrl(url, { accessTokenFactory: () => userStore.token ?? '' })
    .withAutomaticReconnect()
    .build()

  const userStore = useUserStore()

  const isConnected = ref(false)
  const isLoading = ref(false)

  async function connect() {
    isLoading.value = true

    try {
      await connection.start()
      isConnected.value = true
    } catch (error) {
      console.error(error)
      isConnected.value = false
    } finally {
      isLoading.value = false
    }
  }

  async function disconnect() {
    if (connection === null) return
    try {
      await connection.stop()
      isConnected.value = false
    } catch (error) {
      console.error(error)
    }
  }

  async function invokeCommand(commandName: string, ...args: unknown[]) {
    if (connection === null) return
    await connection.invoke(commandName, ...args)
  }

  return {
    connect,
    disconnect,
    invokeCommand,
    connection,
    isConnected,
    isLoading,
  }
}
