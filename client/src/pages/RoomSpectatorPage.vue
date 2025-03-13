<script setup lang="ts">
import axios from 'axios'
import { useQuasar } from 'quasar'
import GameBoardSpectator from 'src/components/gameboard/GameBoardSpectator.vue'
import SpaceButton from 'src/components/general/SpaceButton.vue'
import SpaceCard from 'src/components/general/SpaceCard.vue'
import WaitingButton from 'src/components/general/WaitingButton.vue'
import { useRoomSpectatorStore } from 'src/stores/spectator-room-store'
import { TIMEOUT_LAP, TIMEOUT_PLACEMENT } from 'src/utils/board-utils'
import { computed, onMounted, onUnmounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'

const route = useRoute()
const router = useRouter()
const $q = useQuasar()
const spectatorStore = useRoomSpectatorStore()
const roomGuid = route.params.guid!.toString()

const showDialogEnd = ref<boolean>(false)

watch(
  () => spectatorStore.winnerId,
  (newValue) => {
    if (newValue !== null) {
      showDialogEnd.value = true
    }
  },
)
watch(
  () => spectatorStore.hasBeenCanceled,
  (newValue) => {
    if (newValue) {
      showDialogEnd.value = true
    }
  },
)

async function closeWinnerDialogAndExit() {
  await router.push({ name: 'home' })
}

const labelPlacingTimerButton = computed(() => {
  const invertedTimer = TIMEOUT_PLACEMENT - 1 - spectatorStore.roomPlacingTimer
  return `Temps restant : ${invertedTimer > 0 ? invertedTimer : 0}`
})
const labelLapTimerButton = computed(() => {
  const invertedTimer = TIMEOUT_LAP - 1 - spectatorStore.roomLapTimer
  return `Temps restant : ${invertedTimer > 0 ? invertedTimer : 0}`
})

onMounted(async () => {
  try {
    await spectatorStore.register(roomGuid)
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
onUnmounted(async () => {
  await spectatorStore.unregister()
})
</script>

<template>
  <div class="flex column">
    <q-dialog v-model="showDialogEnd" @hide="closeWinnerDialogAndExit" auto-close>
      <SpaceCard>
        <div class="flex flex-center">{{ spectatorStore.getWinnerString }}</div>
      </SpaceCard>
    </q-dialog>

    <div v-if="spectatorStore.isRoomPlacing" class="flex column">
      <transition appear enter-active-class="animated fadeIn" leave-active-class="animated fadeOut">
        <div class="flex column flex-center">
          <WaitingButton :label="labelPlacingTimerButton" />
          <div v-if="spectatorStore.room" class="flex row flex-center">
            <SpaceButton
              :label="spectatorStore.room?.playerOne.pseudo"
              :disabled="!spectatorStore.playerOneReady"
            />
            <SpaceButton
              :label="spectatorStore.room?.playerTwo?.pseudo ?? '???'"
              :disabled="!spectatorStore.playerTwoReady"
            />
          </div>
        </div>
      </transition>
    </div>

    <div v-if="spectatorStore.isRoomPlaying">
      <transition appear enter-active-class="animated fadeIn" leave-active-class="animated fadeOut">
        <div class="flex column flex-center">
          <WaitingButton :label="labelLapTimerButton" />
          <div class="flex row justify-around" style="width: 90vw">
            <GameBoardSpectator :isPlayerOne="true" />
            <GameBoardSpectator :isPlayerOne="false" />
          </div>
        </div>
      </transition>
    </div>
  </div>
</template>
