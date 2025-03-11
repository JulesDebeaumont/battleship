<script setup lang="ts">
const propsComponents = defineProps<{
  display: string | number
}>()
</script>

<template>
  <div class="gameboard-box gameboard-box-indicator flex flex-center">
    {{ propsComponents.display }}
  </div>
</template>

<style>
.gameboard-box-indicator {
  background-color: #06caca69;
  color: white;
  font-size: larger;
  font-weight: 600;
  backdrop-filter: blur(2px);
}
</style>
