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
      <SpaceButton label="Combat" @click="goRoute('search-room')" />
      <SpaceButton label="Classement" @click="goRoute('leaderboard')" />
      <SpaceButton label="Options" @click="goRoute('me')" />
    </div>
  </div>
</template>
