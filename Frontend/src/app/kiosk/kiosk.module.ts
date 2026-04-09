import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { KioskRoutingModule } from './kiosk-routing.module';
import { KioskComponent } from './kiosk.component';

import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { ToastModule } from 'primeng/toast';
import { AvatarModule } from 'primeng/avatar';
import { TagModule } from 'primeng/tag';
import { DividerModule } from 'primeng/divider';
import { MessageService } from 'primeng/api';

@NgModule({
  declarations: [KioskComponent],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    KioskRoutingModule,
    ButtonModule,
    CardModule,
    InputTextModule,
    ToastModule,
    AvatarModule,
    TagModule,
    DividerModule
  ],
  providers: [MessageService]
})
export class KioskModule {}
