// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// WHY: Fit only an overflowing booking map on narrow screens while preserving the full seat size needed for tapping.
function fitBookingSeatMap() {
    const wrapper = document.querySelector('.seat-wrapper');
    const map = wrapper?.querySelector('.seat-grid');
    if (!wrapper || !map) return;

    map.style.transform = '';
    map.style.transformOrigin = '';
    wrapper.style.height = '';
    wrapper.classList.remove('seat-wrapper-scaled');

    const naturalWidth = map.offsetWidth;
    const naturalHeight = map.offsetHeight;
    // WHY: clientWidth includes the wrapper's padding, so subtract it to fit the grid into the actually visible area.
    const wrapperStyle = getComputedStyle(wrapper);
    const containerWidth = wrapper.clientWidth
        - parseFloat(wrapperStyle.paddingLeft)
        - parseFloat(wrapperStyle.paddingRight);
    if (!naturalWidth || naturalWidth <= containerWidth) {
        wrapper.scrollLeft = 0;
        return;
    }

    const seatWidth = map.querySelector('.booking-seat')?.offsetWidth || 48;
    const fitScale = containerWidth / naturalWidth;
    const minimumScale = 28 / seatWidth;
    const scale = Math.max(fitScale, minimumScale);

    map.style.transformOrigin = 'top left';
    map.style.transform = `scale(${scale})`;
    wrapper.style.height = `${naturalHeight * scale}px`;
    wrapper.classList.add('seat-wrapper-scaled');
    // WHY: Center any remaining overflow when the 28px tap target limit prevents fitting the entire map.
    wrapper.scrollLeft = Math.max(0, (naturalWidth * scale - containerWidth) / 2);
}

// WHY: Recalculate after layout changes so the map fits on initial load, resize, and phone rotation.
let seatMapFitFrame = null;
function scheduleBookingSeatMapFit() {
    if (seatMapFitFrame !== null) cancelAnimationFrame(seatMapFitFrame);
    seatMapFitFrame = requestAnimationFrame(() => {
        seatMapFitFrame = null;
        fitBookingSeatMap();
    });
}

scheduleBookingSeatMapFit();
window.addEventListener('resize', scheduleBookingSeatMapFit);
window.addEventListener('orientationchange', scheduleBookingSeatMapFit);

function requestToken() {
    return document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '';
}

// Keep the cart and the preview focus separate so opening a preview never changes a seat selection.
const selected = new Set();
let focusedId = null;

async function checkSeatAvailability(button) {
    const response = await fetch(`/Book/SelectSeat?id=${parseInt(button.dataset.seatId)}`, {
        method: 'POST',
        headers: { 'RequestVerificationToken': requestToken() }
    });

    if (!response.ok) {
        alert(await response.text() || 'Seat unavailable');
        return false;
    }

    return true;
}

async function handleSeatClick(button) {
    const id = button.dataset.seatId;
    const hasPreview = Boolean(button.dataset.thumb);
    const isBooked = button.dataset.booked === 'true';

    if (selected.has(id)) {
        if (hasPreview) {
            focusedId = id;
            renderSeatSelection();
            return;
        }
        selected.delete(id);
        focusedId = null;
        renderSeatSelection();
        return;
    }

    if (hasPreview) {
        focusedId = id;
        renderSeatSelection();
    }

    // Booked seats can still open their preview, but cannot be selected.
    if (isBooked) return;

    if (!await checkSeatAvailability(button)) return;

    selected.add(id);
    if (!hasPreview) focusedId = null;
    renderSeatSelection();
}

function renderSeatSelection() {
    const seats = [...document.querySelectorAll('.booking-seat')];
    seats.forEach(button => button.classList.toggle('selected', selected.has(button.dataset.seatId)));

    const selectedSeats = seats.filter(button => selected.has(button.dataset.seatId));
    const total = selectedSeats.reduce((sum, button) => sum + parseFloat(button.dataset.price), 0);
    document.getElementById('seat-count').textContent = selectedSeats.length;
    document.getElementById('seat-total').textContent = `BDT ${total.toFixed(2)}`;
    document.getElementById('selected-list').textContent = selectedSeats.length
        ? `Selected: ${selectedSeats.map(button => button.dataset.label).join(', ')}`
        : 'No seats selected';

    const panel = document.getElementById('preview-panel');
    const focusedSeat = seats.find(button => button.dataset.seatId === focusedId && button.dataset.thumb);
    const previewImage = document.getElementById('preview-thumb');
    const previewImageError = document.getElementById('preview-image-error');
    panel.classList.toggle('open', Boolean(focusedSeat));
    panel.setAttribute('aria-hidden', focusedSeat ? 'false' : 'true');
    if (focusedSeat) {
        document.getElementById('preview-title').textContent = `Seat ${focusedSeat.dataset.label}`;
        previewImageError.hidden = true;
        previewImage.onerror = () => {
            previewImage.removeAttribute('src');
            previewImageError.hidden = false;
        };
        previewImage.src = focusedSeat.dataset.thumb;
    }
}

document.addEventListener('click', async event => {
    const button = event.target.closest('.booking-seat');
    if (button) await handleSeatClick(button);
});

document.getElementById('unselect-preview-seat')?.addEventListener('click', () => {
    if (focusedId !== null) selected.delete(focusedId);
    focusedId = null;
    renderSeatSelection();
});

document.getElementById('explore-360')?.addEventListener('click', () => {
    if (focusedId !== null) window.location.href = `/Book/Preview360/${encodeURIComponent(focusedId)}`;
});

if (document.getElementById('preview-panel')) renderSeatSelection();

function bookSelectedSeats() {
    const seatIds = document.getElementById('checkout-seat-ids');
    const form = document.getElementById('checkout-form');
    if (!form) return;

    seatIds.replaceChildren();
    if (selected.size === 0) {
        alert('Select at least one seat before checkout.');
        return;
    }

    for (const id of selected) {
        const input = document.createElement('input');
        input.type = 'hidden';
        input.name = 'seatIds';
        input.value = id;
        seatIds.appendChild(input);
    }

    form.submit();
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

let layoutSeatBeingEdited = null;

function editLayoutSeat(button) {
    layoutSeatBeingEdited = button;
    document.getElementById('layout-seat-label').value = button.dataset.label;
    document.getElementById('layout-seat-price').value = button.dataset.price;
    document.getElementById('layout-seat-image-id').value = button.dataset.id;
    const imagePreview = document.getElementById('layout-seat-image-preview');
    if (button.dataset.img) {
        imagePreview.src = button.dataset.img;
        imagePreview.classList.remove('hidden');
    } else {
        imagePreview.removeAttribute('src');
        imagePreview.classList.add('hidden');
    }
    document.getElementById('layout-seat-form').classList.remove('hidden');
}

async function saveLayoutSeat() {
    const form = document.getElementById('layout-seat-form');
    const label = document.getElementById('layout-seat-label').value.trim();
    const price = Number(document.getElementById('layout-seat-price').value);

    if (!layoutSeatBeingEdited || !label || !Number.isFinite(price) || price <= 0) {
        alert('Enter a seat label and a positive price.');
        return;
    }

    const response = await fetch('/MyLayouts/SaveSeat', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': requestToken()
        },
        body: new URLSearchParams({
            layoutId: form.dataset.layoutId,
            seatId: layoutSeatBeingEdited.dataset.id,
            label: label,
            price: price.toString()
        })
    });

    if (!response.ok) {
        alert(await response.text() || 'Could not update this seat.');
        return;
    }

    layoutSeatBeingEdited.dataset.label = label;
    layoutSeatBeingEdited.dataset.price = price;
    layoutSeatBeingEdited.querySelector('.seat-label').textContent = label;
    layoutSeatBeingEdited.querySelector('.seat-price').textContent = price;
    cancelLayoutSeat();
}

function cancelLayoutSeat() {
    document.getElementById('layout-seat-form').classList.add('hidden');
    layoutSeatBeingEdited = null;
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
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': requestToken()
            },
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
