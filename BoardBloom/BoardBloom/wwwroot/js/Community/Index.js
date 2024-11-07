let timeout;

async function fetchData() {
    const input = document.getElementById("searchInput");
    const url = `/Communities/GetCommunitiesByName?name=${encodeURIComponent(input.value)}`;  

    try {
        const response = await fetch(url);
        if (!response.ok) throw new Error("Network response was not ok");

        const data = await response.text()

        const wrapper = document.getElementById('indexWrapper')
        wrapper.innerHTML = data
    }
    catch (error) {
        console.error("Fetch error:", error);
    }
}

function handleInputChange() {
    if (timeout) clearTimeout(timeout)
    timeout = setTimeout(fetchData, 1000)
}