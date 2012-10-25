
$(document).ready(function () {
	$(".video").each(function () {
		$(this).click(function () {
			var videoID = $(this).attr("video-id");
			$.getJSON("/api/videosapi/GetVideoMediaPlayerSources/?videoid=" + encodeURIComponent(videoID),
				function (data) {
					PlayVideo(data);
				});
		}).mouseenter(function () {
			$(this).addClass("success").css("cursor", "pointer");
		}).mouseleave(function () {
			$(this).removeClass("success").css("cursor", "default");
		});
	});
});
function PlayVideo(sources) {
	$("#videocontainer").empty();
	var player = new PlayerFramework.Player("videocontainer",
	{
		mediaPluginFallbackOrder: ["VideoElementMediaPlugin", "SilverlightMediaPlugin"],
		width: "480px",
		height: "320px",
		sources: sources
	});
	return false;
}
