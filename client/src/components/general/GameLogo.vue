<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'

const route = useRoute()

const classes = computed(() => {
  let classes = 'full-width full-height flex flex-center q-pa-xl logo-container'
  if (route.meta.layoutLogo === 'important') classes += ' logo-container-important'
  return classes
})
</script>

<template>
  <div :class="classes">
    <img src="/images/logo.png" class="title-logo" />
  </div>
</template>

<style>
.logo-container {
  padding-top: 60px;
  transition: all 0.2s;
}
.logo-container-important {
  padding-top: 140px;
}
.title-logo {
  height: 30px;
  z-index: 10;
}
</style>
