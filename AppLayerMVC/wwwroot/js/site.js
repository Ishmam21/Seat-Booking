// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function calculateTotal() {
    const selectedSeats = document.querySelectorAll('.seat.selected');//find buttons and return a js node list of all buttons with class seat and selected
    let total = 0;
    selectedSeats.forEach(seat => {
        total += parseFloat(seat.dataset.price);
    });
    document.getElementById('seat-total').textContent = `Bdt ${total.toFixed(2)}`;
    document.getElementById('seat-count').textContent = selectedSeats.length;
}

async function toggleSeatSelection(btn) {
    const id = parseInt(btn.dataset.id);

    const response = await fetch(`/Home/SelectSeat?id=${id}`, { method: 'POST' });

    if (!response.ok) {
        alert('Seat unavailable');
        return;
    }

    btn.classList.toggle('selected');

    calculateTotal();
}

function lockCell(){
    const cells = document.querySelectorAll('.cell');
    for (const cell of cells) {
        cell.disabled = true;
    }
}

function unlockCell(){
    const cells = document.querySelectorAll('.cell');
    for (const cell of cells) {
        cell.disabled = false;
    }
}

let lastSelected = null;

function selectCell(btn) {
    btn.classList.toggle('selected');
    if (btn.classList.contains('selected')) {
        lastSelected = btn;
        document.getElementById('cell-form').classList.remove('hidden');
        lockCell();
    } else {

        if(confirm('Are you sure you want to deselect this cell?')) {
            btn.textContent = '';
            delete btn.dataset.label;
            delete btn.dataset.price;
            
            }
        else
            {
                btn.classList.add('selected');
            }
    
        }
}

function saveCell() {
    const label = document.getElementById('cell-label').value;
    const priceText = document.getElementById('cell-price').value;
    const price = Number(priceText);

    if (label==='' || isNaN(price) || price < 0) {
        alert('Please fill in all fields with valid values');
        return;
    }

    lastSelected.dataset.label = label;
    lastSelected.dataset.price = price;
    lastSelected.innerHTML = `${label}<br>${price}`;

    closeForm();
}

function cancelCell() {
    if(confirm('Are you sure you want to cancel?')) {
        lastSelected.classList.remove('selected');
        closeForm();
    }
}

function closeForm(){
    document.getElementById('cell-form').classList.add('hidden');
    document.getElementById('cell-label').value = '';
    document.getElementById('cell-price').value = '';
    unlockCell();
}