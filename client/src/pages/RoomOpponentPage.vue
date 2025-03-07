<script setup lang="ts">
import axios from 'axios';
import { useQuasar } from 'quasar';
import GameBoardOpponent from 'src/components/gameboard/GameBoardOpponent.vue';
import GameBoardPlacement from 'src/components/gameboard/GameBoardPlacement.vue';
import GameBoardPlayer from 'src/components/gameboard/GameBoardPlayer.vue';
import { useRoomOpponentStore } from 'src/stores/room-opponent-store'
import type { Component } from 'vue';
import { h, onMounted, onUnmounted, ref, watch } from 'vue'
import { onBeforeRouteLeave, useRoute, useRouter } from 'vue-router'

const route = useRoute()
const router = useRouter()
const $q = useQuasar()
const roomStore = useRoomOpponentStore()
const roomGuid = route.params.guid!.toString()

const showDialogWinner = ref<boolean>(false)
const timer = ref<number>(15)

watch(() => roomStore.winnerId, (newValue) => {
  if (newValue !== null) {
    showDialogWinner.value = true
  }
})
watch(() => roomStore.roomTimer, (newValue) => {
  timer.value = newValue
})

function getComponentByPlacement(placement: 'left' | 'right'): Component {
  if (placement === 'left') {
    return roomStore.isCurrentUserPlayerOne ? GameBoardPlayer : GameBoardOpponent
  }
  if (placement === 'right') {
    return roomStore.isCurrentUserPlayerOne ? GameBoardOpponent : GameBoardPlayer
  }
  return h
}
async function closeWinnerDialogAndExit() {
  await router.push({ name: 'home' })
}

onMounted(async () => {
  try {
    await roomStore.register(roomGuid)
  } catch (error: unknown) {
    if (axios.isAxiosError(error) && error.status === 401) {
      $q.notify({
        type: 'warning',
        message: 'Partie indisponible'
      })
    }
    await router.push({ name: 'home' })
  }
})
onBeforeRouteLeave((to, from, next) => {
  if (roomStore.shouldTriggerAlertWhenLeaving) {
    window.confirm('Vous avez déclaré forfait.')
  }
  next()
})
onUnmounted(async () => {
  await roomStore.unregister()
})
</script>

<template>
  <div>
    {{ roomStore.room }}
    <q-dialog v-model="showDialogWinner" @hide="closeWinnerDialogAndExit" auto-close>
      <q-card class="q-pa-lg">
        <q-card-section>
          {{ roomStore.getWinnerString }}
        </q-card-section>
      </q-card>
    </q-dialog>

    <div v-if="roomStore.isRoomPending">
      <transition appear enter-active-class="animated fadeIn" leave-active-class="animated fadeOut">
        <div>En attente d'un autre joueur..</div>
      </transition>
    </div>

    <div v-if="roomStore.isRoomPlacing" class="flex column">
      <transition appear enter-active-class="animated fadeIn" leave-active-class="animated fadeOut">
        <GameBoardPlacement />
      </transition>
    </div>

    <div v-if="roomStore.isRoomPlaying">
      <transition appear enter-active-class="animated fadeIn" leave-active-class="animated fadeOut">
        <div class="flex column">
          <h6>Chrono : {{ timer }}</h6>
          <div class="flex row">
            <Component :is="getComponentByPlacement('left')" />
            <div class="q-px-xl">_____</div>
            <Component :is="getComponentByPlacement('right')" />
          </div>
        </div>
      </transition>
    </div>

  </div>
</template>
