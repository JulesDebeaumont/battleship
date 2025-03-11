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
  background-color: #0787e349;
  color: white;
  font-size: larger;
  font-weight: 600;
}
</style>
