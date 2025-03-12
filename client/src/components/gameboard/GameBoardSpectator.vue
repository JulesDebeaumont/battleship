<script setup lang="ts">
import GameBoardGeneric from './GameBoardGeneric.vue';
import SpaceButton from '../general/SpaceButton.vue';
import GameBoardSpectatorBox from './boxes/GameBoardSpectatorBox.vue';
import { useRoomSpectatorStore } from 'src/stores/spectator-room-store';

const propsComponents = defineProps<{
    isPlayerOne: boolean
}>()

const spectatorStore = useRoomSpectatorStore()
</script>

<template>
    <div v-if="spectatorStore.room && spectatorStore.room.playerOne && spectatorStore.room.playerTwo"
        class="flex column flex-center">
        <SpaceButton
            :label="propsComponents.isPlayerOne ? spectatorStore.room.playerOne.pseudo : spectatorStore.room.playerTwo.pseudo"
            size="sm" />
        <GameBoardGeneric class="q-mt-sm">
            <template v-slot:boxes="boxSlot">
                <GameBoardSpectatorBox :player-one="propsComponents.isPlayerOne" :xOffset="boxSlot.xOffset"
                    :yOffset="boxSlot.yOffset" />
            </template>
        </GameBoardGeneric>
    </div>
</template>
