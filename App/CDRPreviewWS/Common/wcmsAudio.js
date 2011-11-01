jQuery(document).ready(function() {
    var baseURL = location.href.substring(0, location.href.lastIndexOf("/") + 1);

    var flashPlayer = baseURL + "common/speaker.swf";

    // Rewrite links with a class of either CDR_audiofile (PDQ content) or
    // WCM_audiofile (CancerGov content).
    var audioLinks = jQuery("a[class~='CDR_audiofile'],a[class~='WCM_audiofile']");

    audioLinks.each(function(index, element) {
        var file = element.href;

        // Setup for browers which support both the audio tag and the MP3 file format.
        // Chrome supports MP3, Firefox does not. (But both support the audio tag.)
        if (Modernizr.audio.mp3) {
            // Set up HTML audio
            var audioTag = new Audio(file);
            jQuery(audioTag).insertBefore(element);

            // Make click on the link play the audio, then cancel the click.
            jQuery(element).click(function() {
                audioTag.play();
                return false;
            });
        }
        // For all other browsers, use SWFObject to set up Flash player with fallback to the original tag.
        else {
            // The element should provide an id= attribute.  If not, we'll create one.
            var id = element.id;
            if (id == null || id == "") {
                id = "audio_player_" + index;
                element.id = id;
            }

            var flashvars = {};
            flashvars.sound_name = file;

            var params = {};
            params.wmode = "transparent";

            var attributes = {};
            swfobject.embedSWF(flashPlayer, id, "16", "16", "9.0.28", false, flashvars, params, attributes);
        }

    });
});