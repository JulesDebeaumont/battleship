<script setup lang="ts">
import {
  createRoomAPI,
  ERoomState,
  getAvailableRoomsAPI,
  JoinRoomAsOpponentAPI,
  type IRoomFromListDto,
} from 'src/api/rooms.api'
import SpaceButton from 'src/components/general/SpaceButton.vue'
import SpaceContainer from 'src/components/general/SpaceContainer.vue'
import { useSignalR } from 'src/hooks/use-signalr'
import { useUserStore } from 'src/stores/user-store'
import { onMounted, onUnmounted, ref } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()
const userStore = useUserStore()
const hub = useSignalR('room-list')

const allRooms = ref<IRoomFromListDto[]>([])

async function setupHub() {
  hub.connection.on('RoomListUpdated', async () => {
    await setupAllRoom()
  })

  await hub.connect()
}
async function setupAllRoom() {
  const allNewRooms = await getAvailableRoomsAPI()
  const allNewRoomGuids: string[] = []
  allNewRooms.forEach((newRoom) => {
    allNewRoomGuids.push(newRoom.guid)
    const roomFound = allRooms.value.find((availableRoom) => {
      return availableRoom.guid === newRoom.guid
    })
    if (!roomFound) {
      allRooms.value.push(newRoom)
      return
    }
    roomFound.playerOne = newRoom.playerOne
    roomFound.playerTwo = newRoom.playerTwo
    roomFound.state = newRoom.state
  })
  allRooms.value = allRooms.value.filter((roomAvailable) => {
    return allNewRoomGuids.includes(roomAvailable.guid)
  })
}
async function createRoom() {
  const newRoomGuid = await createRoomAPI()
  await router.push({ name: 'room-as-opponent', params: { guid: newRoomGuid } })
}
async function joinRoomAsOpponent(room: IRoomFromListDto) {
  await JoinRoomAsOpponentAPI(room.guid)
  await router.push({ name: 'room-as-opponent', params: { guid: room.guid } })
}
function canSeeRoomAsOpponent(room: IRoomFromListDto) {
  if (room.state === ERoomState.archived) return false
  if (room.playerOne?.id !== userStore.user!.id && room.playerTwo?.id !== userStore.user!.id)
    return false
  return true
}
function canJoinRoomAsOpponent(room: IRoomFromListDto) {
  if (room.state != ERoomState.pending) return false
  if (room.playerTwo !== null) return false
  if (room.playerOne.id === userStore.user!.id) return false
  return true
}
function canJoinRoomAsSpectator(room: IRoomFromListDto) {
  if (room.playerOne.id === userStore.user!.id || room.playerTwo?.id === userStore.user!.id)
    return false
  if (room.state !== ERoomState.playing) return false
  return true
}

onMounted(async () => {
  await setupAllRoom()
  await setupHub()
})
onUnmounted(async () => {
  await hub.disconnect()
})
</script>

<template>
  <div class="flex column items-center">
    <SpaceButton @click="createRoom" label="Create room" />

    <q-list class="q-py-xl">
      <q-item v-for="room in allRooms" :key="room.guid" class="flex row items-center">
        <SpaceContainer highlitable="none" scan-line="none">
          <div class="text-white text-h6">
            {{ room.playerOne.pseudo }} VS {{ room.playerTwo?.pseudo ?? '???' }}
          </div>
        </SpaceContainer>

        <SpaceButton
          label="Specateur"
          :disabled="!canJoinRoomAsSpectator(room)"
          :to="{ name: 'room-as-spectator', params: { guid: room.guid } }"
          color="secondary"
          size="sm"
        />
        <SpaceButton
          :disabled="!canSeeRoomAsOpponent(room) && !canJoinRoomAsOpponent(room)"
          label="Affronter"
          color="secondary"
          size="sm"
          @click="joinRoomAsOpponent(room)"
        />
      </q-item>
    </q-list>
  </div>
</template>
