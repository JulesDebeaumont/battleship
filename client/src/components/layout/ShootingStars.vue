<script setup lang="ts">
import { uid } from 'quasar';
import { useShootingStarStore } from 'src/stores/shooting-star-store';
import { onMounted, onUnmounted, ref } from 'vue';

interface IShootingStar {
    uid: string
    topPosition: number
    leftPosition: number
    duration: number
}

const TRY_EVERY = 1000 * 1
const PERCENT_CHANCE = 20 / 100
const STARFALL_STAR_COUNT = 30

const starStore = useShootingStarStore()

const shootingStars = ref<IShootingStar[]>([])
const intervalStar = ref<NodeJS.Timeout | null>(null)

function fixedUpdate() {
    const shallHappens = Math.random() <= PERCENT_CHANCE
    if (!shallHappens) return
    spawnStar()
}
function triggerStarFall() {
    for (let i = 0; i < STARFALL_STAR_COUNT; i++) {
        setTimeout(() => {
            spawnStar()
        }, i * 100)
    }
}
function spawnStar() {
    const guid = uid()
    const duration = randomNumberFromInterval(0.8, 3, true)
    const startTop = Math.random() > 0.5
    shootingStars.value.push({
        uid: guid,
        topPosition: startTop ? randomNumberFromInterval(0, 300, false) : 0,
        leftPosition: startTop ? 0 : randomNumberFromInterval(0, 1000, false),
        duration: duration
    })
    setTimeout(() => {
        shootingStars.value = shootingStars.value.filter((star) => {
            return star.uid !== guid
        })
    }, duration * 1000)
}
function randomNumberFromInterval(min: number, max: number, float: boolean) {
    return float ? Math.random() * (max - min + 1) + min : Math.floor(Math.random() * (max - min + 1) + min)
}

onMounted(() => {
    starStore.register(triggerStarFall)
    intervalStar.value = setInterval(() => {
        fixedUpdate()
    }, TRY_EVERY);
})
onUnmounted(() => {
    starStore.unregister()
    if (!intervalStar.value) return
    clearInterval(intervalStar.value)
})
</script>

<template>
    <div class="layout-stars">
        <div v-for="star in shootingStars" :key="star.uid" class="shooting-star"
            :style="` left: initial; top: ${star.topPosition}px; right: ${star.leftPosition}px; animation-duration: ${star.duration}s;`">
        </div>
    </div>
</template>

<style>
.layout-stars {
    position: absolute;
    overflow: hidden;
    height: 100%;
    width: 100%;
    top: 0;
    bottom: 0;
    left: 0;
}

.shooting-star {
    position: absolute;
    top: 50%;
    left: 50%;
    width: 4px;
    height: 4px;
    background: #5fc5bd;
    border-radius: 50%;
    box-shadow: 0 0 0 4px rgba(224, 253, 255, 0.1), 0 0 0 8px rgba(167, 249, 252, 0.1), 0 0 20px rgba(158, 250, 238, 0.1);
    animation: shooting 3s linear;
}

.shooting-star::before {
    content: '';
    position: absolute;
    top: 50%;
    transform: translateY(-50%);
    width: 300px;
    height: 1px;
    background: linear-gradient(90deg, #ccffff, transparent);
}

@keyframes shooting {
    0% {
        transform: rotate(315deg) translateX(0);
        opacity: 1;
    }

    70% {
        opacity: 1;
    }

    100% {
        transform: rotate(315deg) translateX(-1000px);
        opacity: 0;
    }
}
</style>