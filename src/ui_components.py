# src/ui_components.py

import threading
import time
from gi.repository import Gtk, GLib

class UIComponents:
    def __init__(self, parent):
        self.parent = parent
        self.selected_format = None

    def build_header_bar(self):
        header = Gtk.HeaderBar(title="Alkaline")
        header.set_show_close_button(True)
        self.parent.set_titlebar(header)

        # Key Icon Button
        key_icon = Gtk.Image.new_from_icon_name("dialog-password", Gtk.IconSize.BUTTON)
        key_button = Gtk.Button()
        key_button.add(key_icon)
        key_button.connect("clicked", self.on_change_api_key_clicked)
        header.pack_end(key_button)

    def build_main_layout(self):
        vbox = Gtk.Box(orientation=Gtk.Orientation.VERTICAL, spacing=6)
        self.parent.add(vbox)

        # File Label
        label = Gtk.Label(label=f"Convert '{self.parent.file_path}' to:")
        vbox.pack_start(label, False, False, 0)

        # Dropdown Menu
        self.format_combo = Gtk.ComboBoxText()
        self.format_combo.connect("changed", self.on_format_selected)
        vbox.pack_start(self.format_combo, False, False, 0)

        # Convert Button
        self.convert_button = Gtk.Button(label="Convert")
        self.convert_button.set_sensitive(False)
        self.convert_button.connect("clicked", self.parent.on_convert_clicked)
        vbox.pack_start(self.convert_button, False, False, 0)

        # Progress Bar
        self.progress_bar = Gtk.ProgressBar()
        self.progress_bar.set_visible(False)
        vbox.pack_start(self.progress_bar, False, False, 0)

        # Download Button
        self.download_button = Gtk.Button(label="Download")
        self.download_button.set_sensitive(False)
        self.download_button.connect("clicked", self.parent.on_download_clicked)
        self.download_button.set_visible(False)
        vbox.pack_start(self.download_button, False, False, 0)

    def on_change_api_key_clicked(self, button):
        self.show_api_key_prompt(self.parent.on_api_key_entered)

    def show_api_key_prompt(self, callback):
        dialog = Gtk.MessageDialog(parent=self.parent, flags=0, message_type=Gtk.MessageType.QUESTION,
                                   buttons=Gtk.ButtonsType.OK_CANCEL, text="Enter CloudConvert API Key")
        dialog.format_secondary_text("Please enter your CloudConvert API key to proceed.")

        entry = Gtk.Entry()
        entry.set_visibility(False)
        entry.set_invisible_char('*')
        entry.set_activates_default(True)
        dialog.vbox.pack_end(entry, True, True, 0)
        dialog.set_default_response(Gtk.ResponseType.OK)
        dialog.show_all()

        response = dialog.run()
        if response == Gtk.ResponseType.OK:
            key = entry.get_text().strip()
            dialog.destroy()
            callback(key)
        else:
            dialog.destroy()
            self.parent.close()

    def on_format_selected(self, combo):
        format = combo.get_active_text()
        if format:
            self.parent.on_format_selected(format)

    def populate_formats(self, formats):
        for fmt in formats:
            self.format_combo.append_text(fmt)
        if formats:
            self.format_combo.set_active(0)

    def enable_convert_button(self, enable):
        self.convert_button.set_sensitive(enable)

    def set_loading_state(self, loading):
        self.progress_bar.set_visible(loading)
        if loading:
            self.progress_bar.set_fraction(0.0)
            threading.Thread(target=self._update_progress_bar).start()
        else:
            self.progress_bar.set_fraction(0.0)

    def _update_progress_bar(self):
        while self.progress_bar.get_visible():
            current = self.progress_bar.get_fraction()
            new_value = (current + 0.01) % 1.0
            GLib.idle_add(self.progress_bar.set_fraction, new_value)
            time.sleep(0.1)

    def show_download_button(self, show):
        self.download_button.set_visible(show)
        self.download_button.set_sensitive(show)

    def show_error(self, message):
        dialog = Gtk.MessageDialog(parent=self.parent, flags=0, message_type=Gtk.MessageType.ERROR,
                                   buttons=Gtk.ButtonsType.OK, text="Error")
        dialog.format_secondary_text(message)
        dialog.run()
        dialog.destroy()

    def show_info(self, message):
        dialog = Gtk.MessageDialog(parent=self.parent, flags=0, message_type=Gtk.MessageType.INFO,
                                   buttons=Gtk.ButtonsType.OK, text="Success")
        dialog.format_secondary_text(message)
        dialog.run()
        dialog.destroy()
