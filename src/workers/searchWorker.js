import { parentPort, workerData } from 'worker_threads'
import getAlgorithm from '../algorithms/index.js'

const { algorithm, range } = workerData
const algoFunc = getAlgorithm(algorithm)

try {

    if(!algoFunc[algorithm]){
        throw new Error(`Algorithm ${algorithm} not found`)
    }

    const start = performance.now()

    const primes = algoFunc[algorithm](range)

    const end = performance.now()
    const duration = (end - start).toFixed(4)

    parentPort.postMessage({
        status: 'done',
        algorithm: algorithm,
        range: range,
        primesCount: primes.length,
        timeTaken: duration
    })


} catch (error) {
    parentPort.postMessage({
        status: 'error',
        error: error.message
    })
}