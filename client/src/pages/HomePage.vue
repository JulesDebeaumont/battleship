<script setup lang="ts">
import { useSignalR } from 'src/hooks/use-signalr';
import { useRoomStore } from 'src/stores/room-store';

const roomStore = useRoomStore();

const hub = useSignalR(
  'rooms',
  onConnected,
  onDisconnected,
  onReceivedMessage,
  onStopMessage
);

function onConnected() {
  console.log('connected!')
}
function onDisconnected() {
  console.log('disconnected!')
}
function onReceivedMessage(message: unknown) {
  console.log('receive : ')
  console.log(message)
}
function onStopMessage(message: unknown) {
  console.log('onstop message')
  console.log(message)
}

async function letsgo() {
  await hub.connect()
}
</script>

<template>
  <q-page class="column q-px-md q-py-sm">
    <q-btn @click="letsgo" label="ok" />
  </q-page>
</template>
