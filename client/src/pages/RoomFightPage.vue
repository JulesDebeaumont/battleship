<script setup lang="ts">
import axios from 'axios'
import { useQuasar } from 'quasar'
import GameBoardOpponent from 'src/components/gameboard/GameBoardOpponent.vue'
import GameBoardPlacement from 'src/components/gameboard/GameBoardPlacement.vue'
import GameBoardPlayer from 'src/components/gameboard/GameBoardPlayer.vue'
import SpaceCard from 'src/components/general/SpaceCard.vue'
import WaitingButton from 'src/components/general/WaitingButton.vue'
import { useRoomOpponentStore } from 'src/stores/room-opponent-store'
import type { Component } from 'vue'
import { computed, h, onMounted, onUnmounted, ref, watch } from 'vue'
import { onBeforeRouteLeave, useRoute, useRouter } from 'vue-router'

const route = useRoute()
const router = useRouter()
const $q = useQuasar()
const roomStore = useRoomOpponentStore()
const roomGuid = route.params.guid!.toString()

const showDialogWinner = ref<boolean>(false)
const timer = ref<number>(15)

watch(
  () => roomStore.winnerId,
  (newValue) => {
    if (newValue !== null) {
      showDialogWinner.value = true
    }
  },
)
watch(
  () => roomStore.roomTimer,
  (newValue) => {
    timer.value = newValue
  },
)

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

const labelTimerButton = computed(() => {
  const invertedTimer = 14 - timer.value
  if (roomStore.isCurrentUserTurn) return `Temps restant : ${invertedTimer > 0 ? invertedTimer : 0}`
  return "Votre adversaire joue.."
})

onMounted(async () => {
  try {
    await roomStore.register(roomGuid)
  } catch (error: unknown) {
    if (axios.isAxiosError(error) && error.status === 401) {
      $q.notify({
        type: 'warning',
        message: 'Partie indisponible',
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
  <div class="flex column">
    <q-dialog v-model="showDialogWinner" @hide="closeWinnerDialogAndExit" auto-close>
      <SpaceCard>
          <div class="flex flex-center">{{ roomStore.getWinnerString }}</div>
      </SpaceCard>
    </q-dialog>

    <div v-if="roomStore.isRoomPending">
      <transition appear enter-active-class="animated fadeIn" leave-active-class="animated fadeOut">
        <WaitingButton label="En attente d'un autre joueur.." />
      </transition>
    </div>

    <div v-if="roomStore.isRoomPlacing" class="flex column">
      <transition appear enter-active-class="animated fadeIn" leave-active-class="animated fadeOut">
        <GameBoardPlacement />
      </transition>
    </div>

    <div v-if="roomStore.isRoomPlaying">
      <transition appear enter-active-class="animated fadeIn" leave-active-class="animated fadeOut">
        <div class="flex column flex-center">
          <WaitingButton :label="labelTimerButton" />
          <div class="flex row flex-center q-gutter-xl" style="width: 90vw;">
            <Component :is="getComponentByPlacement('left')"  />
            <q-space class="q-px-lg" />
            <Component :is="getComponentByPlacement('right')" />
          </div>
        </div>
      </transition>
    </div>
  </div>
</template>
