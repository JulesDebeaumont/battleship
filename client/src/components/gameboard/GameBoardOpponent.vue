<script setup lang="ts">
import GameBoardOpponentBox from 'src/components/gameboard/boxes/GameBoardOpponentBox.vue'
import { useRoomFightStore } from 'src/stores/room-fight-store';
import GameBoardGeneric from './GameBoardGeneric.vue';
import SpaceButton from '../general/SpaceButton.vue';
const roomStore = useRoomFightStore()
</script>

<template>
    <div class="flex column flex-center">
        <SpaceButton :label="roomStore.getOpponentPseudo" size="sm" />
        <GameBoardGeneric class="q-mt-sm">
            <template v-slot:boxes="boxSlot">
                <GameBoardOpponentBox :xOffset="boxSlot.xOffset" :yOffset="boxSlot.yOffset" />
            </template>
        </GameBoardGeneric>
    </div>
</template>
