import { Injectable } from '@angular/core';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ConfirmDailogComponent } from '../modals/confirm-dailog/confirm-dailog.component';

@Injectable({
  providedIn: 'root'
})
export class ConfirmService {
  bsModelRef?: BsModalRef<ConfirmDailogComponent>;

  constructor(private modelService: BsModalService) { }

  /////////////////// Reusable /////////////////////
  confirm(
    title = 'Confirmation',
    message = 'Are you sure you want to do this?',
    btnOkText = 'Ok',
    btnCancelText = 'Cancel'
  ): Observable<boolean> {
    const config = {
      initialState:{
        title,
        message,
        btnOkText,
        btnCancelText
      }
    }
    this.bsModelRef = this.modelService.show(ConfirmDailogComponent,config);
    return this.bsModelRef.onHidden!.pipe(
      map(() => {
        return this.bsModelRef!.content!.result
      })
    )
  }

}
