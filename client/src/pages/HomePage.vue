<script setup lang="ts">
import SpaceButton from 'src/components/general/SpaceButton.vue'
import { useRouter } from 'vue-router'

const router = useRouter()

async function goRoute(routeName: string) {
  await router.push({ name: routeName })
}
</script>

<template>
  <div class="flex column items-center">
    <div class="flex column">
      <SpaceButton label="Batailles" @click="goRoute('search-room')" />
      <SpaceButton label="Classements" @click="goRoute('leaderboard')" />
      <SpaceButton label="Profil" @click="goRoute('me')" />
    </div>
  </div>
</template>
