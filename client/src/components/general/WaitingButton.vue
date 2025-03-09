<script setup lang="ts">
const propsComponent = defineProps<{
    label: string
}>()
</script>

<template>
    <button class="space-btn space-btn-waiting hologram hologram-waiting">
        <span :data-text="propsComponent.label">{{ propsComponent.label }}</span>
        <div class="scan-line scan-line-waiting"></div>
    </button>
</template>
