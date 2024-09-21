# ui_components.py

import gi
gi.require_version('Gtk', '4.0')
from gi.repository import Gtk

from utils import show_message_dialog

# Global variable to store the selected file path
selected_file_path = None

def create_main_ui(app):
    # Main container
    box = Gtk.Box(orientation=Gtk.Orientation.VERTICAL, spacing=10)
    box.set_margin_top(10)
    box.set_margin_bottom(10)
    box.set_margin_start(10)
    box.set_margin_end(10)

    # API Key Entry
    api_key_entry = Gtk.Entry()
    api_key_entry.set_placeholder_text("Enter your API key")
    api_key_entry.set_hexpand(True)
    api_key_entry.set_text(app.config_manager.get_api_key())

    # Save Button
    save_button = Gtk.Button(label="Save API Key")
    save_button.connect("clicked", on_save_button_clicked, app, api_key_entry)

    # File Selection Button
    file_select_button = Gtk.Button(label="Select a File")
    file_select_button.set_hexpand(True)
    file_select_button.connect("clicked", on_file_select_button_clicked, app)

    # Output Format Entry
    output_format_entry = Gtk.Entry()
    output_format_entry.set_placeholder_text("Enter output format (e.g., pdf)")
    output_format_entry.set_hexpand(True)

    # Convert Button
    convert_button = Gtk.Button(label="Convert File")
    convert_button.connect(
        "clicked",
        on_convert_button_clicked,
        app,
        output_format_entry,
    )

    # Add widgets to box
    box.append(api_key_entry)
    box.append(save_button)
    box.append(file_select_button)
    box.append(output_format_entry)
    box.append(convert_button)

    # Create a scrolled window in case content overflows
    scrolled_window = Gtk.ScrolledWindow()
    scrolled_window.set_child(box)
    scrolled_window.set_policy(Gtk.PolicyType.NEVER, Gtk.PolicyType.AUTOMATIC)

    return scrolled_window

def on_save_button_clicked(button, app, api_key_entry):
    api_key = api_key_entry.get_text()
    app.config_manager.set_api_key(api_key)
    app.cloudconvert_api.set_api_key(api_key)
    show_message_dialog(app, "API Key Saved", "Your API key has been saved.")

def on_file_select_button_clicked(button, app):
    global selected_file_path

    dialog = Gtk.FileChooserNative(
        title="Select a File",
        transient_for=app.get_active_window(),
        action=Gtk.FileChooserAction.OPEN,
    )

    def response_handler(dialog, response):
        global selected_file_path
        if response == Gtk.ResponseType.ACCEPT:
            selected_file = dialog.get_file()
            if selected_file:
                selected_file_path = selected_file.get_path()
                button.set_label(f"Selected: {selected_file.get_basename()}")
        dialog.destroy()

    dialog.connect("response", response_handler)
    dialog.show()

def on_convert_button_clicked(button, app, output_format_entry):
    global selected_file_path
    output_format = output_format_entry.get_text().strip()
    if selected_file_path and output_format:
        app.cloudconvert_api.convert_file(selected_file_path, output_format)
        show_message_dialog(app, "Conversion Started", "Your file is being converted.")
    else:
        show_message_dialog(app, "Missing Information", "Please select a file and specify the output format.")
