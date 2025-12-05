const connection = new signalR.HubConnectionBuilder().withUrl("/RaceHub").build()

let algorithms = []
let selectedAlgorithms = []
let chartInstance = null
let latestData = []
let renderInterval = null
let currentHttpController = null;

const chartColors = [
    'rgba(255, 99, 132, 0.8)',
    'rgba(54, 162, 235, 0.8)',
    'rgba(255, 206, 86, 0.8)',
    'rgba(75, 192, 192, 0.8)',
    'rgba(153, 102, 255, 0.8)',
    'rgba(255, 159, 64, 0.8)'
]

// --- BUTTONS ---

function addAlgorithm(){
    let input = document.getElementById('algoSearch')
    let value = input.value.trim()

    const algoObj = algorithms.find(a => a.label == value)

    if(!algoObj){
        alert('Такого алгоритму не існує, виберіть алгоритм зі списку')
        return
    }

    if(selectedAlgorithms.find(a => a.key == algoObj.key)){
        input.value = ''
        alert('Цей алгоритм вже доданий')
        return
    }

    if(selectedAlgorithms.length >= 3){
        input.value = ''
        alert('Обрана максимальна кількість алгоритмів')
        return
    }

    const assignedColor = chartColors[selectedAlgorithms.length % chartColors.length];
    
    selectedAlgorithms.push({
        ...algoObj,
        color: assignedColor
    })

    renderSelectedList()
    input.value = ''
}

function deleteAlgoFromList(key){
    selectedAlgorithms = selectedAlgorithms.filter(a => a.key != key)
    renderSelectedList()
}

async function startRace(){
    const range = document.getElementById('inputRange').value

    if(selectedAlgorithms.length < 2){
        alert('Спочатку оберіть агоритми (min. 2)')
        return
    }

    if (currentHttpController) {
        currentHttpController.abort()
    }

    currentHttpController = new AbortController()

    document.getElementById('startRace').disabled = true

    finishedCount = 0

    latestData = new Array(selectedAlgorithms.length).fill(0)

    chartInstance.data.labels = selectedAlgorithms.map(a => [a.label , ''])
    chartInstance.data.datasets[0].backgroundColor = selectedAlgorithms.map(a => a.color)
    chartInstance.data.datasets[0].data = latestData
    chartInstance.update()

    if (renderInterval) clearInterval(renderInterval)
    
    renderInterval = setInterval(() => {
        chartInstance.data.datasets[0].data = latestData
        chartInstance.update('none')
    }, 100)

    try {
        const algoNames = selectedAlgorithms.map(a => a.key)

        await fetch('/api/Race/start', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            signal: currentHttpController.signal,
            body: JSON.stringify({ limit: parseInt(range), selectedAlgorithms: algoNames })
        })
    } catch (error) {
        if (error.name === 'AbortError') {
            console.log("Гонку скасовано користувачем");
        } else {
            console.error("Failed to start race:", error);
            alert("Помилка запуску гонки");
        }   
    }finally{
        currentHttpController = null
    }

}

function resetResult(){

    if (currentHttpController) {
        currentHttpController.abort()
        currentHttpController = null
    }

    if (renderInterval) {
        clearInterval(renderInterval)
        renderInterval = null
    }

    latestData = new Array(selectedAlgorithms.length).fill(0)
    finishedCount = 0

    chartInstance.data.datasets[0].data = latestData

    chartInstance.data.labels = chartInstance.data.labels.map(label => {
        if (Array.isArray(label)){
            label[1] = ''
            return label
        }

        return label
    })

    chartInstance.update()

    

    document.getElementById('startRace').disabled = false
}

// --- OTHER STUFF ---

async function getAlgorithms(){
    try {
        const response = await fetch('/api/Race/algorithms')

        if(!response){
            throw new Error('Fetch to get algorithms')
        
        }

        algorithms = await response.json()

        initAlgoOptionsList()

    } catch (error) {
        console.error(error)
    }
    
}

function renderSelectedList(){
    const listContainer = document.getElementById('selected-list')
    listContainer.innerHTML = ''

    if (selectedAlgorithms.length === 0) {
        listContainer.innerHTML = `
            <div class="text-center text-muted p-3 bg-light rounded border border-dashed small">
                Список порожній. Додайте алгоритми вище.
            </div>`
        return
    }

    selectedAlgorithms.forEach(algo => {
        const item = document.createElement('div')
        item.className = 'list-group-item d-flex justify-content-between align-items-center animate-fade'

        item.innerHTML = 
        `
            <span class="fw-bold" style="color: #333;">${algo.label}</span>
            <button class="btn btn-sm btn-outline-danger border-0 delbtn" onclick="deleteAlgoFromList('${algo.key}')" title="Видалити">
                ✕
            </button>
        `

        listContainer.appendChild(item)
    })
}

async function initAlgoOptionsList(){
    const algoOptions = document.getElementById('algoOptions')
    algoOptions.innerHTML = ''

    algorithms.forEach(algo => {
        const option = document.createElement('option')
        option.value = algo.label

        algoOptions.append(option)
    })
}

function initChart(){
    const ctx = document.getElementById('raceChart').getContext('2d')

    chartInstance = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: [],
            datasets: [{
                label: 'Result in percentage',
                data: [],
                borderWidth: 1
            }]
        },
        options: {
            animation: false,
            scales: {
                y: {
                    beginAtZero: true,
                    max: 100
                }
            }
        }
    })
}

function stopRaceLoop(){
    if (renderInterval) {
        clearInterval(renderInterval)
        renderInterval = null
    }

    chartInstance.data.datasets[0].data = latestData
    chartInstance.update('none')
    
    document.getElementById('startRace').disabled = false
}

async function startApp(){
    try {
        await connection.start();
        console.log("SignalR Connected!");
        document.getElementById('startRace').disabled = false
    } catch (err) {
        console.error("SignalR Connection Error: ", err)
        setTimeout(startApp, 5000);
    }
}

// --- SIGNAL HANDLERS ---

connection.on('ReceiveProgress', (data) => {
    const index = selectedAlgorithms.findIndex(a => a.label === data.algorithm);
    
    if (index !== -1) {
        latestData[index] = data.progress;
    }

    if (data.progress > currentProgress) {
            latestData[index] = data.progress;
    }
})

connection.on('AlgorithmFinished', (data) => {
    const chartIndex = chartInstance.data.labels.findIndex(label => {
        if (Array.isArray(label)){
            return label[0] == data.algorithm
        }

        return label == data.algorithm
    })

    const index = selectedAlgorithms.findIndex(a => a.label === data.algorithm)

    let timeTaken = data.timeTaken
    let timeTakenText
    
    if (index !== -1) {
        latestData[index] = 100
        finishedCount++
        
        if (finishedCount >= selectedAlgorithms.length) {
            stopRaceLoop()
        }

        if (timeTaken > 1000){
            timeTakenText = `${(timeTaken / 1000).toFixed(3)} sec`
        }else{
            timeTakenText = `${timeTaken.toFixed(3)} ms`
        }

        chartInstance.data.labels[chartIndex] = [data.algorithm, timeTakenText]
        chartInstance.update()
    }
})

function findCharIndex(){
    
}

// --- INIT PROGRAM ---

document.addEventListener('DOMContentLoaded', () =>{
    initChart()
    getAlgorithms()
    startApp()
})

// --- BUTTONS EVENTS

document.getElementById('startRace').onclick = startRace
document.getElementById('addAlgorithmBtn').onclick = addAlgorithm
document.getElementById('ResetResult').onclick = resetResult