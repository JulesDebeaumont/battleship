<script setup lang="ts">
import { useRoomOpponentStore } from 'src/stores/room-opponent-store';
import GameBoardUserBox from './boxes/GameBoardUserBox.vue';
import GameBoardGeneric from './GameBoardGeneric.vue';

const roomStore = useRoomOpponentStore()
</script>

<template>
    <div class="flex q-pa-md bg-pink-1">
        PLAYER {{ roomStore.getUserPseudo }}
        <GameBoardGeneric>
            <template v-slot:boxes="boxSlot">
                <GameBoardUserBox :xOffset="boxSlot.xOffset" :yOffset="boxSlot.yOffset" />
            </template>
        </GameBoardGeneric>
    </div>
</template>
