#!/bin/bash

# List of component files that need standalone: false
components=(
  "src/app/app.component.ts"
  "src/app/components/inbox/inbox.component.ts"
  "src/app/components/calendar/calendar.component.ts"
  "src/app/components/calendar/event-dialog/event-dialog.component.ts"
  "src/app/components/contacts/contacts.component.ts"
  "src/app/components/contacts/contact-dialog/contact-dialog.component.ts"
  "src/app/components/video-conference/video-conference.component.ts"
  "src/app/components/language-switcher/language-switcher.component.ts"
  "src/app/components/auth/login/login.component.ts"
  "src/app/components/auth/register/register.component.ts"
  "src/app/components/auth/unauthorized/unauthorized.component.ts"
  "src/app/components/layout/main-layout/main-layout.component.ts"
  "src/app/components/correspondence-dashboard/correspondence-dashboard.component.ts"
  "src/app/components/correspondence-list/correspondence-list.component.ts"
  "src/app/components/correspondence-detail/correspondence-detail.component.ts"
  "src/app/components/correspondence-form/correspondence-form.component.ts"
  "src/app/components/correspondence-routing-dialog/correspondence-routing-dialog.component.ts"
  "src/app/components/archive-management/archive-management.component.ts"
)

for file in "${components[@]}"; do
  if [ -f "$file" ]; then
    echo "Processing $file..."
    # Add standalone: false to @Component decorator if not already present
    if ! grep -q "standalone:" "$file"; then
      # Find @Component({ and add standalone: false after the opening brace
      sed -i '/@Component({/,/})/ {
        /@Component({/ {
          a\  standalone: false,
        }
      }' "$file"
    fi
  else
    echo "File not found: $file"
  fi
done

echo "Done!"
