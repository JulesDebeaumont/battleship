<script setup lang="ts">
import { createRoomAPI, getAvailableRoomsAPI, type IRestrictedRoomDto } from 'src/api/rooms.api'
import { useSignalR } from 'src/hooks/use-signalr'
import { onMounted, onUnmounted, ref } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()
const hub = useSignalR('room-list', onConnected, onDisconnected, onReceivedMessage, onStopMessage)

const allRooms = ref<IRestrictedRoomDto[]>([])

async function onConnected() {
  await setupAllRoom()
}
function onDisconnected() {
  console.log('disconnected!')
}
async function onReceivedMessage(message: unknown) {
  console.log('receive : ')
  console.log(message)
  await setupAllRoom()
}
function onStopMessage(message: unknown) {
  console.log('onstop message')
  console.log(message)
}
async function setupAllRoom() {
  allRooms.value = await getAvailableRoomsAPI()
}
async function createRoom() {
  const newRoomGuid = await createRoomAPI()
  await router.push({ name: 'room', params: { guid: newRoomGuid, role: 'opponent' } })
}

onMounted(async () => {
  await hub.connect()
})
onUnmounted(async () => {
  await hub.disconnect()
})
</script>

<template>
  <q-page class="column q-px-md q-py-sm">
    <q-spinner v-if="hub.isLoading.value" />
    <div v-if="!hub.isLoading.value && !hub.isConnected.value">Pas connecter :></div>
    <q-list>
      <q-item v-for="room in allRooms" :key="room.guid">
        <pre>{{ room }}</pre>
      </q-item>
    </q-list>
    <q-btn @click="createRoom" label="Create room" />
  </q-page>
</template>
