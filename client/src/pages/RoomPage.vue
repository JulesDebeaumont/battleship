<script setup lang="ts">
import { getRoomByGuidAPI, IRestrictedRoomDto, leaveRoomAPI } from 'src/api/rooms.api';
import { useSignalR } from 'src/hooks/use-signalr';
import { onMounted, onUnmounted, ref } from 'vue';
import { useRoute } from 'vue-router';

type TRoomRole = 'opponent' | 'spectator'

const hub = useSignalR(
  'rooms-on',
  onConnected,
  onDisconnected,
  onReceivedMessage,
  onStopMessage
);
const route = useRoute()
const roomGuid = route.params.guid!.toString()
const role = route.params.role!.toString() as TRoomRole;

const room = ref<IRestrictedRoomDto>();

async function onConnected() {
}
function onDisconnected() {
}
async function onReceivedMessage(message: unknown) {
  console.log('receive : ')
  console.log(message)
}
function onStopMessage(message: unknown) {
}
function isRoleCorrect() {
    return role === 'opponent' || role === 'spectator'
}
async function setupRoom() {
    room.value = await getRoomByGuidAPI(roomGuid);
}

onMounted( async () => {
    if (isRoleCorrect()) {
        return;
    }
    await setupRoom()
    await hub.connect()
    if (role === 'opponent') {
        hub.invokeCommand('JoinRoomOpponentChannel', roomGuid)
    }
    if (role === 'spectator') {
        hub.invokeCommand('JoinRoomSpectatorChannel', roomGuid)
    }
})
onUnmounted( async () => {
    await leaveRoomAPI(roomGuid);
})
</script>

<template>
    sdfgsdf
</template>