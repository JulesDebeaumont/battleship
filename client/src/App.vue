<template>
  <div v-if="isLoadingServerToken" class="max-width max-height bg-indigo flex flex-center">
    <h1>Authentification en cours...</h1>
  </div>
  <MainLayout v-else />
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue';
import MainLayout from './layouts/MainLayout.vue';
import { useUserStore } from './stores/user-store';

const userStore = useUserStore();

const isLoadingServerToken = ref(false);

async function checkForTicketInUrl() {
  const urlWithTicket = window.location.search.match(/ST.+/)
  if (!userStore.isConnected && urlWithTicket !== null && (urlWithTicket?.length ?? 0 > 0)) {
    const ticketFromCas = urlWithTicket[0]
    isLoadingServerToken.value = true;
    await userStore.askServerToken(ticketFromCas);
    isLoadingServerToken.value = false;
  }
}

onMounted(async () => {
  if (!userStore.isConnected) {
    userStore.getInitialPathRequested();
    await userStore.trySetupTokenFromCookies();
  }
  await checkForTicketInUrl();
})
</script>
