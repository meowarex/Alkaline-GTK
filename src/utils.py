# utils.py

import gi
gi.require_version('Gtk', '4.0')
from gi.repository import Gtk

def show_message_dialog(app, title, message):
    dialog = Gtk.MessageDialog(
        transient_for=app.get_active_window(),
        modal=True,
        message_type=Gtk.MessageType.INFO,
        buttons=Gtk.ButtonsType.OK,
        text=title,
    )
    dialog.format_secondary_text(message)
    dialog.connect("response", lambda d, r: d.destroy())
    dialog.show()
