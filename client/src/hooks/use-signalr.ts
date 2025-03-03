import type { HubConnection } from '@microsoft/signalr'
import { HubConnectionBuilder } from '@microsoft/signalr'
import { useUserStore } from 'src/stores/user-store'
import { ref } from 'vue'

export function useSignalR(
  hubName: string,
  onConnect: () => Promise<void> | void,
  onDisconnect: () => Promise<void>  | void,
  onReceiveMessage: (message: unknown) => Promise<void>  | void,
  onStopMessage: (message: unknown) => Promise<void>  | void,
) {
  let connection: HubConnection | null = null

  const userStore = useUserStore()

  const isConnected = ref(false)

  async function connect() {
    const url = `${process.env.ENDPOINT_API}/ws/${hubName}`
    connection = new HubConnectionBuilder()
      .withUrl(url, { accessTokenFactory: () => userStore.token ?? '' })
      .withAutomaticReconnect()
      .build()

    connection.on('ReceiveMessage', async (message) => {
      await onReceiveMessage(message)
    })

    connection.on('StopMessage', async (message) => {
      await onStopMessage(message)
    })

    try {
      await connection.start()
      await onConnect()
      isConnected.value = true
    } catch (error) {
      console.error(error)
      isConnected.value = false
    }
  }

  async function disconnect() {
    if (connection === null) return
    try {
      await connection.stop()
      await onDisconnect()
      isConnected.value = false
    } catch (error) {
      console.error(error)
    }
  }

  async function invokeCommand(commandName: string, ...args: unknown[]) {
    if (connection === null) return;
    await connection.invoke(commandName, ...args)
  }

  return {
    connect,
    disconnect,
    invokeCommand,
    connection,
    isConnected: isConnected.value,
  }
}
