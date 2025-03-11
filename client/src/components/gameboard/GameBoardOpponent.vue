<script setup lang="ts">
import GameBoardOpponentBox from 'src/components/gameboard/boxes/GameBoardOpponentBox.vue'
import { useRoomOpponentStore } from 'src/stores/room-opponent-store';
import GameBoardGeneric from './GameBoardGeneric.vue';
import SpaceButton from '../general/SpaceButton.vue';
const roomStore = useRoomOpponentStore()
</script>

<template>
    <div class="flex q-pa-md column flex-center">
        <SpaceButton :label="roomStore.getOpponentPseudo" size="sm" />
        <GameBoardGeneric class="q-mt-sm">
            <template v-slot:boxes="boxSlot">
                <GameBoardOpponentBox :xOffset="boxSlot.xOffset" :yOffset="boxSlot.yOffset" />
            </template>
        </GameBoardGeneric>
    </div>
</template>
