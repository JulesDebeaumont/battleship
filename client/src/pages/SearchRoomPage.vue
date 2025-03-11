<script setup lang="ts">
import type { QTableColumn } from 'quasar'
import {
  createRoomAPI,
  ERoomState,
  getAvailableRoomsAPI,
  joinRoomAsOpponentAPI,
  type IRoomFromListDto,
} from 'src/api/rooms.api'
import HologramText from 'src/components/general/HologramText.vue'
import SpaceButton from 'src/components/general/SpaceButton.vue'
import { useSignalR } from 'src/hooks/use-signalr'
import { useUserStore } from 'src/stores/user-store'
import { onMounted, onUnmounted, ref } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()
const userStore = useUserStore()
const hub = useSignalR('room-list')
const columnsRooms: QTableColumn[] = [
  {
    name: 'index',
    label: '#',
    align: 'left',
    field: 'index',
  },
  {
    name: 'playerOne',
    label: 'Joueur 1',
    align: 'left',
    field: 'playerOne',
  },
  {
    name: 'playerTwo',
    label: 'Joueur 2',
    align: 'left',
    field: 'playerTwo',
  },
  {
    name: 'state',
    label: 'État',
    align: 'left',
    field: 'state',
  },
  {
    name: 'actions',
    label: 'Actions',
    align: 'center',
    field: 'actions',
  },
];

const allRooms = ref<IRoomFromListDto[]>([])
const isLoadingCreateRoom = ref<boolean>(false)

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
  isLoadingCreateRoom.value = true
  const newRoomGuid = await createRoomAPI()
  await router.push({ name: 'room-as-opponent', params: { guid: newRoomGuid } })
}
async function joinRoomAsOpponent(room: IRoomFromListDto) {
  await joinRoomAsOpponentAPI(room.guid)
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
  if (room.state !== ERoomState.playing && room.state !== ERoomState.placing) return false
  return true
}
function getLabelRoomState(state: ERoomState) {
  const labelByState: Record<ERoomState, string> = {
    '0': 'En cours',
    '1': 'En attente',
    '2': 'Placement',
    '3': 'Archive'
  }
  return labelByState[state]
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
  <div class="flex column items-center full-width full-height">
    <SpaceButton @click="createRoom" label="Créer bataille" :disabled="isLoadingCreateRoom" />

    <transition v-if="!isLoadingCreateRoom && allRooms.length > 0" appear enter-active-class="animated fadeIn"
      leave-active-class="animated fadeOut">

      <div class="flex column items-center q-mb-md full-width full-height">

        <q-table :rows="allRooms" :columns="columnsRooms" class="space-table" row-key="id"
          table-header-class="space-table-header" hide-bottom :pagination="{ rowsPerPage: 1000 }">
          <template v-slot:body-cell-index="props">
            <q-td :props="props">
              <span class="text-body1">{{ props.rowIndex }}</span>
            </q-td>
          </template>
          <template v-slot:body-cell-playerOne="props">
            <q-td :props="props">
              <HologramText :text="props.row.playerOne.pseudo" class="text-body1" />
            </q-td>
          </template>
          <template v-slot:body-cell-playerTwo="props">
            <q-td :props="props">
              <HologramText :text="props.row.playerTwo?.pseudo ?? ''" class="text-body1" />
            </q-td>
          </template>
          <template v-slot:body-cell-state="props">
            <q-td :props="props">
              <span class="text-body1">{{ getLabelRoomState(props.row.state) }}</span>
            </q-td>
          </template>
          <template v-slot:body-cell-actions="props">
            <q-td :props="props">
              <div class="flex row justify-end items-center">
                <SpaceButton label="Specateur" :disabled="!canJoinRoomAsSpectator(props.row)"
                  @click="router.push({ name: 'room-as-spectator', params: { guid: props.row.guid } })" size="sm"
                  color="secondary" />
                <SpaceButton :disabled="!canSeeRoomAsOpponent(props.row) && !canJoinRoomAsOpponent(props.row)"
                  label="Affronter" size="sm" color="secondary" @click="joinRoomAsOpponent(props.row)" />
              </div>
            </q-td>
          </template>
        </q-table>
      </div>
    </transition>
    <SpaceButton size="sm" label="Retour" @click="router.push({ name: 'home' })" />
  </div>
</template>
