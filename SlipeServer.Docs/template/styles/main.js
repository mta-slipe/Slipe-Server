
// load contributors from github API
$(document).ready(() => {
	$(".js-contributor-list").each(async (index, element) => {
		const contributors = await $.get("https://api.github.com/repos/mta-slipe/Slipe-Server/contributors");
		
		const contributorList = $(element);
		for (const contributor of contributors) {
			const elementString = "<li><a href=\"" + contributor.html_url + "\" target=\"_blank\">" + contributor.login + "</a></li>";
			contributorList.append($(elementString));
		}
	});

	// This is necesary since the sidebar toc is  asynchronously loaded after document ready
	setTimeout(() => {
		$('.sidetoc .nav a[title*=".On"]').hide();
	}, 1000);

	for (let element of document.querySelectorAll(".js-platform-link")) {
		let newHref = element.getAttribute("data-href-" + navigator.platform.toLowerCase())
		if (newHref) {
			element.setAttribute("href", newHref);
		}
	}

	$(".js-download-button").on("click", (event, element) => {
		$(".download-modal-container").toggle()
	})
	$(".download-modal-container").on("click", (event, element) => {
		$(".download-modal-container").toggle()
	})
	$(".download-modal").on("click", (event, element) => {
		event.stopPropagation();
	})

	$(".js-prod-link").each((index, link) => {
		if (document.location.href.indexOf("development.") > 0) {
			link.href = link.href.replace("-production", "-development");
		}
	});
});
