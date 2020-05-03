/*TOKEN*/
const getToken = () => {
    let cookies = document.cookie;
    if (cookies) {
        return document.cookie.split('=')[1];
    }
    else {
        console.warn("There are no cookies");
        return undefined;
    }
}

/*GLOBAL VARIABLES*/
const token = getToken();
const page = document.getElementsByClassName("body-content")[0];
const loadSpinnerDiv = document.getElementById("loadSpinner");
const loadSpinnerClass = "load-spinner";
const tableHeaders = ["Order Number", "Product", "Quantity", "Price", "Payment", "Status"];
const allOrderStatuses = ["Pending", "In Progress", "Completed", "Shipped"];
const allPaymentStatuses = ["Pending", "Received"];
const updateStatusEndpoint = "/api/Order/status";
const getSummaryEndpoint = "/api/Order/summary";

/*TABLE CREATION*/
const getSummary = () => {
    if (token) {
        $.ajax({
            type: "GET",
            contentType: "application/json; charset=utf-8",
            headers: {
                'Authorization': 'Bearer ' + token
            },
            url: getSummaryEndpoint,
            success: (data) => {
                displayTables(data);
                setupStatusListeners();
                removeloadSpinnerAnimation();
            },
            error: () => {
                console.log("Something went wrong with the orders");
                removeloadSpinnerAnimation();
            }
        })
    }
    else {
        console.warn("No token found")
    }
}

const displayTables = (data) => {
    const pending = data.filter(o => o.Status === allOrderStatuses[0]);
    const inProgress = data.filter(o => o.Status === allOrderStatuses[1]);
    const completed = data.filter(o => o.Status === allOrderStatuses[2]);
    const shipped = data.filter(o => o.Status === allOrderStatuses[3]);

    setupTable("pending", pending);
    setupTable("inprogress", inProgress);
    setupTable("completed", completed);
    setupTable("shipped", shipped);
}

const setupTable = (tableName, specificData) => {
    let table = document.getElementById(tableName);
    generateTableHead(table, tableHeaders);

    if (Array.isArray(specificData) && specificData.length > 0) {
        let modifiedOrders = modifyOrders(specificData);
        generateTable(table, modifiedOrders);
    }

    CheckIfTableEmpty(table);
}

const generateTableHead = (table, headers) => {
    let thead = table.createTHead();
    let row = thead.insertRow();
    for (let key of headers) {
        let th = document.createElement("th");
        let text = document.createTextNode(key);
        th.appendChild(text);
        row.appendChild(th);
    }
}

const generateTable = (table, data) => {
    for (let element of data) {
        let row = table.insertRow();
        let values = Object.values(element);
        for (const [i, value] of values.entries()) {
            let cell = row.insertCell();
            addColumnClasses(i, value, cell);
            let text = getCellContents(i, value);
            cell.appendChild(text);
            if (i == 4 || i == 5) {
                let dropdownMenu = createDropdown(i);
                cell.appendChild(dropdownMenu);
            }
        }
    }
}

const addColumnClasses = (i, value, cell) => {
    if (i == 0) {
        cell.className = "orderNumberCol";
    }
    else if (i == 1) {
        cell.className = "productCol";
    }
    else if (i == 2) {
        cell.className = "quantityCol";
    }
    else if (i == 3) {
        cell.className = "priceCol";      
    }
    else if (i == 4 || i == 5) {
        cell.className = i == 4 ? `paymentStatusCol-${value.replace(/\s+/g, '')}` : `orderStatusCol-${value.replace(/\s+/g, '')}`;       
    }
}

const getCellContents = (i, value) => {
    if (i < 4) {
        return document.createTextNode(value);
    }
    else if (i == 4 || i == 5) {
        let text = document.createTextNode(value);
        let div = document.createElement("div");
        div.appendChild(text);
        return div;
    }
}

const modifyOrders = (data) => {
    const modified = data.map(o => {
        return {
            "Order Number": o.OrderNumber,
            "Product": o.ProductName,
            "Quantity": o.Quantity,
            "Price": `$${o.TotalPrice}`,
            "Payment": o.Paid,
            "Status": o.Status
        };
    });
    return modified;
}

/*DROPDOWN MENU*/
const createDropdown = (column) => {
    let listOfText = column == 4 ? allPaymentStatuses : allOrderStatuses;
    let isOrderStatus = column == 4 ? false : true;

    let dropdownUl = document.createElement("ul");
    dropdownUl.className = "dropdown-content";

    listOfText.forEach((text) => {
        let li = document.createElement("li");
        let a = document.createElement("a");
        let aText = document.createTextNode(text);

        a.className = column == 4 ? `paymentStatusDropdown-${text}` : `orderStatusDrop-${text.replace(/\s+/g, '')}`;
        a.appendChild(aText);
        setupChosenStatusListener(a, isOrderStatus);
        li.appendChild(a);
        dropdownUl.appendChild(li);
    });

    return dropdownUl;
}

/*STATUS CHANGE REQUEST*/
const setupStatusListeners = () => {
    let orderStatuses = document.querySelectorAll("td[class*='orderStatusCol'] div");
    let paymentStatuses = document.querySelectorAll("td[class*='paymentStatusCol'] div");
    orderStatuses.forEach((status) => {
        status.addEventListener('click', function (event) {
            toggleDropdown(event, this);
        }, false);
    });
    paymentStatuses.forEach((status) => {
        status.addEventListener('click', function () {
            toggleDropdown(event, this);
        }, false);
    });
}

const setupChosenStatusListener = (element, isOrderStatus) => {
    element.addEventListener('click', (event) => {
        let statusText = updateAndGetDropdownSelection(event, isOrderStatus);
        if (statusText == null)
            return;
        let orderNum = getOrderNumberAndUpdateTables(event, isOrderStatus);
        //putStatusChangeRequest(orderNum, statusText, isOrderStatus);
    }, false);
}

const toggleDropdown = (event, element) => {
    let isToggled = $(element).siblings()[0].classList.contains("show");

    if (!isToggled) {
        $(element).siblings()[0].classList.toggle("show");
        event.stopPropagation();
        page.addEventListener('click', function hideDropdown(e) {
            let allDropdowns = document.querySelectorAll("ul");
            allDropdowns.forEach((dropdown) => {
                dropdown.classList.remove("show");
            });
            e.srcElement.removeEventListener('click', hideDropdown);
            e.stopPropagation();
        }, false);
    }
    else {
        $(element).siblings()[0].classList.remove("show");
    }
}

const updateAndGetDropdownSelection = (event, isOrderStatus) => {
    let chosenStatus = $(event.currentTarget).text();
    let parentTd = $(event.currentTarget)[0].offsetParent.parentElement;

    if (parentTd.firstElementChild.textContent == chosenStatus)
        return null;

    parentTd.className = isOrderStatus ? parentTd.className = `orderStatusCol-${chosenStatus.replace(/\s+/g, '')}`
                                       : parentTd.className = `paymentStatusCol-${chosenStatus.replace(/\s+/g, '')}`;

    let newStatusText = document.createTextNode(chosenStatus);
    parentTd.firstElementChild.replaceChild(newStatusText, parentTd.firstElementChild.childNodes[0]);

    return chosenStatus;
}

const getOrderNumberAndUpdateTables = (event, isOrderStatus) => {
    let currElement = $(event.currentTarget)[0];
    let row = findSpecificParent(currElement, "tr");
    let orderNumber = row.firstElementChild.textContent

    if (isOrderStatus) {
        updateTableRows(currElement, row);
    }

    return orderNumber
}

const updateTableRows = (currElement, row) => {
    let table = findSpecificParent(row, "table");
    let newTable = document.getElementById(currElement.text.toLowerCase().replace(/\s+/g, ''));

    table.firstElementChild.deleteRow(row.rowIndex);
    newTable.firstElementChild.appendChild(row);
    CheckIfTableEmpty(table);
    CheckIfTableEmpty(newTable);
}

const putStatusChangeRequest = (orderNumber, status, isOrderStatus) => {
    if (token) {
        let statusChangeRequest = {
            OrderNumber: orderNumber,
            Status: status,
            IsOrderStatus: isOrderStatus ? "Y" : "N"
        };
        $.ajax({
            type: "PUT",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify(statusChangeRequest),
            headers: {
                'Authorization': 'Bearer ' + token
            },
            url: updateStatusEndpoint,
            success: () => {
                alert("Successful status change");
            },
            error: () => {
                console.log("Something went wrong with changing status of order");
            }
        })
    }
    else {
        console.warn("No token found")
    }
}

/*UTILITIES*/
const isDescendant = (parent, child) => {
    let node = child.parentNode;
    while (node != null) {
        if (node == parent) {
            return true;
        }
        node = node.parentNode;
    }
    return false;
}

const CheckIfTableEmpty = (table) => {
    let rows = table.querySelectorAll("tr");
    if (rows.length < 2)
        table.parentNode.style.display = "none";
    else
        table.parentNode.style.display = "block";
}

const findSpecificParent = (el, tagName) => {
    tagName = tagName.toLowerCase();

    while (el && el.parentNode) {
        el = el.parentNode;
        if (el.tagName && el.tagName.toLowerCase() == tagName) {
            return el;
        }
    }
}

/*STARTUP*/
const addloadSpinnerAnimation = () => {
    loadSpinnerDiv.classList.add(loadSpinnerClass);
}

const removeloadSpinnerAnimation = () => {
    loadSpinnerDiv.classList.remove(loadSpinnerClass);
}

document.addEventListener('DOMContentLoaded', () => {
    console.log("Page Started");
    addloadSpinnerAnimation();
    getSummary();
    console.log("Page Loaded");

}, false);