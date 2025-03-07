<script setup lang="ts">
import GameBoardOpponentBox from 'src/components/gameboard/boxes/GameBoardOpponentBox.vue'
import { useRoomOpponentStore } from 'src/stores/room-opponent-store';
import GameBoardGeneric from './GameBoardGeneric.vue';

const roomStore = useRoomOpponentStore()
</script>

<template>
    <div class="flex q-pa-md bg-pink-1"> OPPONENT {{ roomStore.getOpponentPseudo }}
        <GameBoardGeneric>
            <template v-slot:boxes="boxSlot">
                <GameBoardOpponentBox :xOffset="boxSlot.xOffset" :yOffset="boxSlot.yOffset" />
            </template>
        </GameBoardGeneric>
    </div>
</template>
