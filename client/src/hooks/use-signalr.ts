import type { HubConnection } from '@microsoft/signalr'
import { HubConnectionBuilder } from '@microsoft/signalr'
import { ref } from 'vue'

export function useSignalR(
  hubName: string,
  onConnect: () => void,
  onDisconnect: () => void,
  onReceiveMessage: (message: unknown) => void,
  onStopMessage: (message: unknown) => void,
) {
  let connection: HubConnection | null = null

  const isConnected = ref(false)

  async function connect() {
    const url = `${process.env.API}/ws/${hubName}`
    connection = new HubConnectionBuilder().withUrl(url).withAutomaticReconnect().build()

    onConnect()

    connection.on('ReceiveMessage', (message) => {
      onReceiveMessage(message)
    })

    connection.on('StopMessage', (message) => {
      onStopMessage(message)
    })

    try {
      await connection.start()
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
      onDisconnect()
      isConnected.value = false
    } catch (error) {
      console.error(error)
    }
  }

  return {
    connect,
    disconnect,
    isConnected: isConnected.value,
  }
}
