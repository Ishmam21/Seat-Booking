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
        // document.getElementById('cell-label').focus();
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

    if (label==='' || isNaN(price) || price <= 0) {
        alert('Please fill in all fields with valid values');
        return;
    }

    lastSelected.dataset.label = label;
    lastSelected.dataset.price = price;
    lastSelected.textContent = `${label}\n${price}`;

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

function resetLayout() {
    if(confirm('Are you sure you want to reset the layout? This will clear all cells.')) {
        const cells = document.querySelectorAll('.cell');
        for (const cell of cells) {
            cell.textContent = '';
            delete cell.dataset.label;
            delete cell.dataset.price;
            cell.classList.remove('selected');
        }

        closeForm();
    }
}

async function submitLayout() {
    
    const formIsOpen = !document.getElementById('cell-form').classList.contains('hidden');
    if (formIsOpen) {
    alert('Finish the open seat first');
    return;
    }

    const layoutName = prompt('Enter a name for the layout:');
    if (!layoutName || layoutName.trim() === '') {
        alert('Please enter a valid layout name.');
        return;
    }

    const activeCells = document.querySelectorAll('.cell.selected');

    if (activeCells.length === 0) {
        alert('Select minimum one cell before submitting the layout.');
        return;
    }


    const seats = [];
    for (const cell of activeCells) {
        seats.push({
            label: cell.dataset.label,
            price: parseFloat(cell.dataset.price),
            x: parseInt(cell.dataset.col),
            y: parseInt(cell.dataset.row)
        });
    }

    const response = await fetch('/Creator/SaveLayout',
        {
            method: 'POST',
            headers: {'Content-Type': 'application/json'},
            body: JSON.stringify(
                {
                    name: layoutName.trim(),
                    seats: seats
                }
            )
        }

    );

    if (!response.ok) {
        alert('Error submitting layout');
        return;
    }


    alert(`Layout submitted: ${layoutName}`);
    window.location.href = '/Select/Index';
}

function backConfirm()
{
    if(confirm('Are you sure you want to go back? All unsaved changes will be lost.')) {
        return history.back();
    }
}
