import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component, DestroyRef,
  EventEmitter,
  inject,
  Input,
  OnInit,
  Output
} from '@angular/core';
import {CommonModule} from '@angular/common';
import {FormControl, FormGroup, ReactiveFormsModule} from "@angular/forms";
import {Select2Module} from "ng-select2-component";
import {TranslocoDirective} from "@jsverse/transloco";
import {takeUntilDestroyed} from "@angular/core/rxjs-interop";
import {debounceTime, distinctUntilChanged, tap} from "rxjs/operators";

@Component({
  selector: 'app-edit-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, Select2Module, TranslocoDirective],
  templateUrl: './edit-list.component.html',
  styleUrl: './edit-list.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EditListComponent implements OnInit {

  private readonly cdRef = inject(ChangeDetectorRef);
  private readonly destroyRef = inject(DestroyRef);

  @Input({required: true}) items: Array<string> = [];
  @Input({required: true}) label = '';
  @Output() updateItems = new EventEmitter<Array<string>>();

  form: FormGroup = new FormGroup({});
  /**
   * Names of the form controls backing each row, in display order. Rows are tracked individually (not as a
   * comma-joined string) so an item containing a comma (e.g. the exclude pattern "Bonus, Extras") stays one row.
   */
  controlKeys: Array<string> = [];
  private nextControlId = 0;


  ngOnInit() {
    const items = this.items.length > 0 ? this.items : [''];
    items.forEach(item => this.addControl(item));

    this.form.valueChanges.pipe(
      debounceTime(100),
      distinctUntilChanged(),
      tap(data => this.emit()),
      takeUntilDestroyed(this.destroyRef))
    .subscribe();
    this.cdRef.markForCheck();
  }

  add() {
    this.addControl('');
    this.emit();
    this.cdRef.markForCheck();
  }

  remove(index: number) {
    if (index == 0 && this.controlKeys.length === 1) {
      this.form.get(this.controlKeys[0])?.setValue('', {emitEvent: true});
      this.emit();
      this.cdRef.markForCheck();
      return;
    }

    const key = this.controlKeys[index];
    this.controlKeys = this.controlKeys.filter((_, i) => i !== index);
    this.form.removeControl(key, {emitEvent: true});

    this.emit();
    this.cdRef.markForCheck();
  }

  emit() {
    this.updateItems.emit(this.controlKeys
    .map(key => this.form.get(key)?.value)
    .filter(v => v !== null && v !== ''));
  }

  private addControl(value: string) {
    const key = 'link' + this.nextControlId++;
    this.form.addControl(key, new FormControl(value, []));
    this.controlKeys = [...this.controlKeys, key];
  }
}
