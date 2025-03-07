<script setup lang="ts">
import type { IRoomSpectatorDto } from 'src/api/rooms.api'
import { getRoomAsSpectatorByGuidAPI, leaveRoomAPI } from 'src/api/rooms.api'
import { useSignalR } from 'src/hooks/use-signalr'
import { onMounted, onUnmounted, ref } from 'vue'
import { useRoute } from 'vue-router'

const hub = useSignalR('rooms-on')
const route = useRoute()
const roomGuid = route.params.guid!.toString()

const room = ref<IRoomSpectatorDto>()
async function setupRoom() {
  room.value = await getRoomAsSpectatorByGuidAPI(roomGuid)
}

onMounted(async () => {
  await setupRoom()
  await hub.connect()
  await hub.invokeCommand('JoinRoomSpectatorChannel', roomGuid)
})
onUnmounted(async () => {
  await hub.disconnect()
  await leaveRoomAPI(roomGuid)
})
</script>

<template>
  <div>
    {{ room }}

  </div>
</template>
