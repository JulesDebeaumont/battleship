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
